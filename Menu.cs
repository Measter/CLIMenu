using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CLIMenu
{
  public class Menu
  {
    public override string ToString()
    {
      return string.Format( "Name: {0}, Items: {1}", Name, Items.Count );
    }

    /// <summary>
    /// The index of the currently selected item.
    /// </summary>
    public int SelectedIndex
    {
      get;
      set;
    }
    /// <summary>
    /// The name of the menu. This is displayed in the title of the console window.
    /// </summary>
    public string Name
    {
      get;
      set;
    }
    /// <summary>
    /// Sets whether the selection star should be displayed.
    /// </summary>
    public bool ShowSelected
    {
      get;
      set;
    }
    /// <summary>
    /// The minimum size of the console window.
    /// </summary>
    public Size MinSize
    {
      get;
      set;
    }
    /// <summary>
    /// The maximum size of the console window.
    /// </summary>
    public Size MaxSize
    {
      get;
      set;
    }
    /// <summary>
    /// The list of items in the menu.
    /// </summary>
    public MenuItemCollection Items
    {
      get;
      private set;
    }
    /// <summary>
    /// The array of border characters. Stored in the following order:
    /// <para>Top Left</para>
    /// <para>Top Right</para>
    /// <para>Bottom Right</para>
    /// <para>Bottom Left</para>
    /// <para>Horizontal</para>
    /// <para>Vertical</para>
    /// </summary>
    public Char[] BorderChars
    {
      get;
      set;
    }
    /// <summary>
    /// The character used to indicate the current selection index.
    /// </summary>
    public Char SelectionIndicator
    {
      get;
      set;
    }
    /// <summary>
    /// A list of item indices that should be skipped over.
    /// </summary>
    public List<int> BannedIndices
    {
      get;
      private set;
    }

    /// <summary>
    /// This event is fired if a button is pressed that is not one of the following:
    /// <para>Up Arrow</para>
    /// <para>Down Arrow</para>
    /// <para>Escape</para>
    /// <para>Enter</para>
    /// </summary>
    public event onOtherButtonHandler onOtherButton;

    private bool m_exiting = false;
    private Size m_size = new Size();
    private bool m_isShown = false;
    private bool m_isScrolling = false;

    private int[] m_firstLast = new int[2];

    /// <summary>
    /// Creates a new Menu.
    /// </summary>
    public Menu()
    {
      ShowSelected = true;

      BorderChars = new[] { '\u2554', '\u2557', '\u255D', '\u255A', '\u2550', '\u2551' };
      SelectionIndicator = '*';

      Items = new MenuItemCollection();
      BannedIndices = new List<int>();
    }
    /// <summary>
    /// Creates a new Menu.
    /// </summary>
    /// <param name="name">The name of the menu. Displayed in console title.</param>
    public Menu( string name )
      : this()
    {
      Name = name;
    }

    /// <summary>
    /// Displays the menu.
    /// </summary>
    public void Show()
    {
      m_exiting = false;
      SelectedIndex = 0;

      SetScreenSize();

      while( BannedIndices.Contains( SelectedIndex ) )
      {
        SelectedIndex++;
        if( SelectedIndex == Items.Count )
          return;
      }

      while( true )
      {
        if( m_exiting )
          break;

        DrawScreen();

        Input();
      }
    }

    private void SetScreenSize()
    {
      int temp = 0;

      foreach( MenuItem item in Items )
        if( item.Name.Length > temp )
          temp = item.Name.Length;

      if( ShowSelected )
        m_size.Width = temp + 8;
      else
        m_size.Width = temp + 6;

      m_size.Height = Items.Count + 6;

      //Size checking.
      if( MinSize.Height > m_size.Height )
        m_size.Height = MinSize.Height;
      else if( MaxSize.Height >= 5 && MaxSize.Height < m_size.Height )
      {
        m_size.Height = MaxSize.Height;
        m_isScrolling = true;
      }

      if( m_isScrolling )
        m_size.Width++;

      if( MinSize.Width > m_size.Width )
        m_size.Width = MinSize.Width;
      else if( MaxSize.Width >= 5 && MaxSize.Width < m_size.Width )
        m_size.Width = MaxSize.Width;
    }

    private void Input()
    {
      ConsoleKeyInfo cin = Console.ReadKey();

      switch( cin.Key )
      {
        case ConsoleKey.UpArrow:
          HandleUp();
          break;
        case ConsoleKey.DownArrow:
          HandleDown();
          break;
        case ConsoleKey.Escape:
          m_exiting = true;
          break;
        case ConsoleKey.Enter:
          m_isShown = false;
          Items[SelectedIndex].FireClick( cin );
          break;
        default:
          onOtherButton( this, new onOtherButtonHandlerArgs( Items[SelectedIndex], cin ) );
          break;
      }
    }

    private void HandleDown()
    {
      int prev = SelectedIndex;

      SelectedIndex++;

      if( SelectedIndex == Items.Count )
      {
        SelectedIndex = Items.Count - 1;
        return;
      }

      while( BannedIndices.Contains( SelectedIndex ) )
      {
        SelectedIndex++;
        if( SelectedIndex == Items.Count )
        {
          SelectedIndex = prev;
          break;
        }
      }
    }

    private void HandleUp()
    {
      int prev = SelectedIndex;

      SelectedIndex--;

      if( SelectedIndex == -1 )
      {
        SelectedIndex = 0;
        return;
      }

      while( BannedIndices.Contains( SelectedIndex ) )
      {
        SelectedIndex--;
        if( SelectedIndex == -1 )
        {
          SelectedIndex = prev;
          break;
        }
      }
    }

    private void DrawScreen()
    {
      if( !m_isShown )
      {
        //Setting the window size.
        Console.SetWindowSize( 1, 1 );
        Console.SetBufferSize( m_size.Width, m_size.Height );
        Console.SetWindowSize( m_size.Width, m_size.Height );

        if( Name != String.Empty )
          Console.Title = Name;
        Console.CursorVisible = false;

        m_isShown = true;
      }

      Console.Clear();

      DrawBorder();
      if( !m_isScrolling )
      {
        DrawItems();
      } else
      {
        DrawScrollingItems();
        DrawScrollBar();
      }
    }

    private void DrawScrollBar()
    {
      float a, c, range = m_size.Height - 7;

      a = (float)SelectedIndex / (float)( Items.Count - 1 );
      c = a * range;

      Console.SetCursorPosition( m_size.Width - 3, (int)c + 3 );
      Console.Write( "|" );
    }

    private void DrawScrollingItems()
    {
      SetFirstLast();

      int startLine = 3;
      for( int i = 0; i < Items.Count; i++ )
      {
        if( i <= m_firstLast[1] && i >= m_firstLast[0] )
        {
          DrawItem( startLine, i );

          startLine++;
        }
      }
    }

    private void SetFirstLast()
    {
      int maxItems = m_size.Height - 7;

      if( SelectedIndex == 0 )
      {
        m_firstLast[0] = 0;
        m_firstLast[1] = maxItems;
      } else if( SelectedIndex == Items.Count - 1 )
      {
        m_firstLast[0] = SelectedIndex - maxItems;
        m_firstLast[1] = SelectedIndex;
      } else
      {
        if( SelectedIndex > m_firstLast[1] - 1 )
        {
          m_firstLast[0]++;
          m_firstLast[1]++;
        }
        if( SelectedIndex < m_firstLast[0] + 1 )
        {
          m_firstLast[0]--;
          m_firstLast[1]--;
        }
      }
    }

    private void DrawItems()
    {
      int startLine = 3;

      for( int i = 0; i < Items.Count; i++ )
      {
        DrawItem( startLine + i, i );
      }
    }

    private void DrawItem( int drawLine, int i )
    {
      StringBuilder sb;
      sb = new StringBuilder();
      if( ShowSelected )
        sb.Append( i == SelectedIndex && ShowSelected ? " " + SelectionIndicator + " " : "   " );
      else
        sb.Append( " " );
      sb.Append( Items[i].Name );

      if( sb.Length > m_size.Width - 5 )
      {
        string t = sb.ToString();
        sb = new StringBuilder();
        sb.Append( t.Substring( 0, m_size.Width - 8 ) );
        sb.Append( "..." );
      }

      Console.SetCursorPosition( 2, drawLine );
      Console.Write( sb.ToString() );
    }

    private void DrawBorder()
    {
      StringBuilder sb;

      //Draw top border
      sb = new StringBuilder();
      sb.Append( BorderChars[0] );
      sb.Append( "".PadLeft( m_size.Width - 4, BorderChars[4] ) );
      sb.Append( BorderChars[1] );

      Console.SetCursorPosition( 1, 1 );
      Console.Write( sb.ToString() );

      //Draw side borders
      for( int i = 2; i < m_size.Height - 1; i++ )
      {
        Console.SetCursorPosition( 1, i );
        Console.Write( BorderChars[5] );
        Console.SetCursorPosition( m_size.Width - 2, i );
        Console.Write( BorderChars[5] );
      }

      //Draw bottom border
      sb = new StringBuilder();
      sb.Append( BorderChars[3] );
      sb.Append( "".PadLeft( m_size.Width - 4, BorderChars[4] ) );
      sb.Append( BorderChars[2] );

      Console.SetCursorPosition( 1, m_size.Height - 2 );
      Console.Write( sb.ToString() );
    }
  }

  public class MenuItemCollection : List<MenuItem>
  {
    public new void Add( MenuItem item )
    {
      base.Add( item );
    }

    public new void AddRange( IEnumerable<MenuItem> items )
    {
      base.AddRange( items );
    }

    public new void Clear()
    {
      base.Clear();
    }

    public new void Insert( int index, MenuItem item )
    {
      base.Insert( index, item );
    }

    public new void InsertRange( int index, IEnumerable<MenuItem> items )
    {
      base.InsertRange( index, items );
    }

    public new void Remove( MenuItem item )
    {
      base.Remove( item );
    }

    public new void RemoveAll( Predicate<MenuItem> match )
    {
      base.RemoveAll( match );
    }

    public new void RemoveAt( int index )
    {
      base.RemoveAt( index );
    }

    public new void RemoveRange( int index, int count )
    {
      base.RemoveRange( index, count );
    }
  }

  /// <summary>
  /// A menu item for use in the Menu object.
  /// </summary>
  public class MenuItem
  {
    public override string ToString()
    {
      return string.Format( "Name: {0}", Name );
    }

    /// <summary>
    /// The name of the item. This is the text displayed in a menu.
    /// </summary>
    public string Name
    {
      get;
      set;
    }

    /// <summary>
    /// Stores any custom data that is needed.
    /// </summary>
    public Dictionary<string, object> CustomData
    {
      get;
      private set;
    }

    /// <summary>
    /// This event will fire when the user presses the Enter button on this event.
    /// </summary>
    public event MenuItemClickedEventHandler onClick;

    /// <summary>
    /// Creates a menu item.
    /// </summary>
    /// <param name="name">The name of the item.</param>
    public MenuItem( string name )
    {
      CustomData = new Dictionary<string, object>();
      Name = name;
    }
    /// <summary>
    /// Creates a menu item.
    /// </summary>
    /// <param name="name">The name of the item.</param>
    /// <param name="click">The method used to handle input.</param>
    public MenuItem( string name, MenuItemClickedEventHandler click )
      : this( name )
    {
      onClick += click;
    }

    /// <summary>
    /// Fires the onClick event.
    /// </summary>
    /// <param name="key">The key used to fire the event.</param>
    public virtual void FireClick( ConsoleKeyInfo key )
    {
      if( onClick != null )
        onClick( this, key );
    }
  }

  public delegate void MenuItemClickedEventHandler( MenuItem item, ConsoleKeyInfo key );

  public delegate void onOtherButtonHandler( Menu sender, onOtherButtonHandlerArgs args );

  public class onOtherButtonHandlerArgs
  {
    /// <summary>
    /// The selected menu item at the time the onOtherButton event was fired.
    /// </summary>
    public MenuItem MenuItem
    {
      get;
      private set;
    }

    /// <summary>
    /// The key used to fire the onOtherButton event.
    /// </summary>
    public ConsoleKeyInfo Key
    {
      get;
      private set;
    }

    public onOtherButtonHandlerArgs( MenuItem menuItem, ConsoleKeyInfo key )
    {
      MenuItem = menuItem;
      Key = key;
    }
  }
}