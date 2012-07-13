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
    /// Sets whether the window should be resized on the next render cycle.
    /// </summary>
    public bool ForceReset
    {
      get;
      set;
    }
    /// <summary>
    /// Exits the menu on the next loop if true;
    /// </summary>
    public bool Exit
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
    /// Whether the selection should be indicated by reversing the colours or with a character.
    /// </summary>
    public bool InvertedIndicator { get; set; }
    /// <summary>
    /// The character used to indicate the scrollbar postion.
    /// </summary>
    public Char ScrollbarCharacter
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
    /// The foreground colour used for rendering.
    /// </summary>
    public ConsoleColor Foreground
    {
      get;
      set;
    }
    /// <summary>
    /// The background colour used for rendering.
    /// </summary>
    public ConsoleColor Background
    {
      get;
      set;
    }

    /// <summary>
    /// This event is fired after clearing the screen, but before rendering begins.
    /// </summary>
    public event PreRenderHandler PreRender;
    /// <summary>
    /// This event is fired after rendering.
    /// </summary>
    public event PostRenderHandler PostRender;

    private Size m_size = new Size();
    private bool m_isScrolling = false;

    private int[] m_firstLast = new int[2];  //The first and last item IDs for a scrolling menu.
    private int m_maxItems;
    private int m_indexChange = 0;  //Keeps track of how much the item index has changed.

    private const int BORDER_WIDTH = 3;
    private const int DOUBLE_BORDER_WIDTH = BORDER_WIDTH * 2;

    /// <summary>
    /// Creates a new Menu.
    /// </summary>
    public Menu()
    {
      ShowSelected = true;
      ForceReset = true;

      Exit = false;

      BorderChars = new[] { '\u2554', '\u2557', '\u255D', '\u255A', '\u2550', '\u2551' };
      SelectionIndicator = '*';
      ScrollbarCharacter = '|';
      InvertedIndicator = false;
      Background = ConsoleColor.Black;
      Foreground = ConsoleColor.Gray;

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
      Exit = false;
      SelectedIndex = 0;

      SetScreenSize();

      //Find the first item ID that isn't in the banned list.
      while( BannedIndices.Contains( SelectedIndex ) )
      {
        SelectedIndex++;
        if( SelectedIndex == Items.Count )
          return;
      }

      //Recalculate the maximum items for scrolling, and reset the FirstLast position.
      if( m_isScrolling )
      {
        m_maxItems = m_size.Height - ( DOUBLE_BORDER_WIDTH + 1 );

        m_firstLast[0] = SelectedIndex;
        m_firstLast[1] = SelectedIndex + m_maxItems;
      }

      //Input and draw loop.
      while( !Exit )
      {
        DrawScreen();

        Input();
      }
    }

    private void SetScreenSize()
    {
      int longestItemLength = 0;

      //Find the longest item name.
      foreach( MenuItem item in Items )
        if( item.ToString().Length > longestItemLength )
          longestItemLength = item.ToString().Length;

      //If showing the selected item, add 2 characters to give room for the indicator.
      if( ShowSelected && !InvertedIndicator )
        m_size.Width = longestItemLength + DOUBLE_BORDER_WIDTH + 2;
      else
        m_size.Width = longestItemLength + DOUBLE_BORDER_WIDTH;

      m_size.Height = Items.Count + DOUBLE_BORDER_WIDTH;

      //Size checking.
      if( MinSize.Height > m_size.Height )
        m_size.Height = MinSize.Height;
      else if( MaxSize.Height >= 5 && MaxSize.Height < m_size.Height )
      {
        m_size.Height = MaxSize.Height;
        m_isScrolling = true;
      }

      if( m_isScrolling )
        m_size.Width++; //Add a space for the scroll bar.

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
          Exit = true;
          ForceReset = true;
          break;
        default:
          Items[SelectedIndex].FireKeyPress( this, cin );
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

      //If the new selected index is in the banned list, iterate over them until
      //an index is found that isn't banned. If the end of the list is reached,
      //return to the previous index.
      while( BannedIndices.Contains( SelectedIndex ) )
      {
        SelectedIndex++;
        if( SelectedIndex == Items.Count )
        {
          SelectedIndex = prev;
          break;
        }
      }

      m_indexChange = SelectedIndex - prev;
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

      //If the new selected index is in the banned list, iterate over them until
      //an index is found that isn't banned. If the end of the list is reached,
      //return to the previous index.
      while( BannedIndices.Contains( SelectedIndex ) )
      {
        SelectedIndex--;
        if( SelectedIndex == -1 )
        {
          SelectedIndex = prev;
          break;
        }
      }

      m_indexChange = SelectedIndex - prev;
    }

    private void DrawScreen()
    {
      //The screen should only be resized if control has been moved away from
      //the menu.
      if( ForceReset )
      {
        //Setting the window size.
        Console.SetWindowSize( 1, 1 );
        Console.SetBufferSize( m_size.Width, m_size.Height );
        Console.SetWindowSize( m_size.Width, m_size.Height );

        if( !string.IsNullOrEmpty( Name ) )
          Console.Title = Name;
        Console.CursorVisible = false;

        Console.ForegroundColor = Foreground;
        Console.BackgroundColor = Background;

        ForceReset = false;
      }

      Console.Clear();

      if( PreRender != null )
        PreRender( this );

      DrawBorder();
      if( !m_isScrolling )
      {
        DrawItems();
      } else
      {
        DrawScrollingItems();
        DrawScrollBar();
      }

      if( PostRender != null )
        PostRender( this );
    }

    private void DrawScrollBar()
    {
      float a, c, range = m_size.Height - ( DOUBLE_BORDER_WIDTH + 1 );

      //Calculate the position of the scroll bar.
      a = (float)SelectedIndex / (float)( Items.Count - 1 );
      c = a * range;

      Console.SetCursorPosition( m_size.Width - BORDER_WIDTH, (int)c + BORDER_WIDTH );
      Console.Write( ScrollbarCharacter );
    }

    private void DrawScrollingItems()
    {
      //Get the first and last item indices to show.
      if( SelectedIndex > m_firstLast[1] || SelectedIndex < m_firstLast[0] )
      {
        m_firstLast[0] += m_indexChange;
        m_firstLast[1] += m_indexChange;
      }

      int drawLine = BORDER_WIDTH;
      for( int i = 0; i < Items.Count; i++ )
      {
        if( i <= m_firstLast[1] && i >= m_firstLast[0] )
        {
          DrawItem( drawLine, i );

          drawLine++;
        }
      }
    }

    private void DrawItems()
    {
      for( int i = 0; i < Items.Count; i++ )
        DrawItem( BORDER_WIDTH + i, i );
    }

    private void DrawItem( int drawLine, int i )
    {
      ConsoleColor fg = Foreground, bg = Background;
      
      StringBuilder sb = new StringBuilder();
      if( ShowSelected && !InvertedIndicator )
        sb.Append( i == SelectedIndex && ShowSelected ?  SelectionIndicator + " " : "  " );

      sb.Append( Items[i].ToString() );

      //Check the length of the name to see if it needs clipping.
      if( sb.Length > m_size.Width - DOUBLE_BORDER_WIDTH )
      {
        string t = sb.ToString();
        sb = new StringBuilder();
        sb.Append( t.Substring( 0, m_size.Width - ( DOUBLE_BORDER_WIDTH + BORDER_WIDTH ) ) );
        sb.Append( "..." );
      }

      //Need to flip the colours if using an inverted indicator.
      if ( InvertedIndicator && i == SelectedIndex )
      {
        Console.ForegroundColor = Background;
        Console.BackgroundColor = Foreground;
      }

      Console.SetCursorPosition( BORDER_WIDTH, drawLine );
      Console.Write( sb.ToString() );

      //Now we need to restore the colours.
      if( InvertedIndicator && i == SelectedIndex )
      {
        Console.ForegroundColor = Foreground;
        Console.BackgroundColor = Background;
      }
    }

    private void DrawBorder()
    {
      StringBuilder sb;

      //Draw top border
      sb = new StringBuilder();
      sb.Append( BorderChars[0] );
      sb.Append( "".PadLeft( m_size.Width - ( BORDER_WIDTH + 1 ), BorderChars[4] ) );
      sb.Append( BorderChars[1] );

      Console.SetCursorPosition( 1, 1 );
      Console.Write( sb.ToString() );

      //Draw side borders
      for( int i = 2; i < m_size.Height - 1; i++ )
      {
        Console.SetCursorPosition( ( BORDER_WIDTH - 2 ), i );
        Console.Write( BorderChars[5] );
        Console.SetCursorPosition( m_size.Width - ( BORDER_WIDTH - 1 ), i );
        Console.Write( BorderChars[5] );
      }

      //Draw bottom border
      sb = new StringBuilder();
      sb.Append( BorderChars[3] );
      sb.Append( "".PadLeft( m_size.Width - ( BORDER_WIDTH + 1 ), BorderChars[4] ) );
      sb.Append( BorderChars[2] );

      Console.SetCursorPosition( 1, m_size.Height - ( BORDER_WIDTH - 1 ) );
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
  }

  /// <summary>
  /// A menu item for use in the Menu object.
  /// </summary>
  public abstract class MenuItem
  {
    /// <summary>
    /// This event will fire when the user presses the Enter button on this event.
    /// </summary>
    public event MenuItemClickedEventHandler onKeyPress;

    /// <summary>
    /// Fires the onKeyPress event.
    /// </summary>
    /// <param name="key">The key used to fire the event.</param>
    public bool FireKeyPress( Menu sender, ConsoleKeyInfo key )
    {
      if( onKeyPress != null )
      {
        onKeyPress( sender, new onKeyPressArgs( this, key ) );
        return true;
      }

      return false;
    }
  }

  public delegate void MenuItemClickedEventHandler( Menu sender, onKeyPressArgs args );

  public delegate void PreRenderHandler( Menu sender );

  public delegate void PostRenderHandler( Menu sender );

  public class onKeyPressArgs
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

    public onKeyPressArgs( MenuItem menuItem, ConsoleKeyInfo key )
    {
      MenuItem = menuItem;
      Key = key;
    }
  }
}