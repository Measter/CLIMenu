using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using CLIMenu;

namespace CLIMenuExample
{
  class Program
  {
    private static Menu m_mainMenu, m_listDemoMenu, m_sumMenu, m_scrollingDemo;
    private static SumItem First, Second, Sum;

    static void Main( string[] args )
    {
      //Lets create the list demo menu.
      SetupListDemo();

      //Setting up the sum menu.
      SetupSumMenu();

      //Setting up the scrolling demo
      SetupScrollDemo();

      //Now we'll create the main menu.
      SetupMainMenu();

      //Now that the menues have been set up, we'll show the main menu.
      m_mainMenu.Show();
    }

    private static void SetupScrollDemo()
    {
      m_scrollingDemo = new Menu( "Scrolling Demo" );

      m_scrollingDemo.MaxSize = new Size( 80, 15 );

      for( int i = 0; i < 20; i++ )
        m_scrollingDemo.Items.Add( new ListItem( "Item " + i ) );

      m_scrollingDemo.BannedIndices.AddRange( new[] { 2, 3, 5, 6 } );

      m_scrollingDemo.ScrollbarCharacter = '+';
    }

    private static void SetupListDemo()
    {
      //Because this is a list, we'll hide the selection star.
      m_listDemoMenu = new Menu( "List Demo" );
      m_listDemoMenu.ShowSelected = false;

      //We'll also set a maximum size to demonstrate the clipping.
      m_listDemoMenu.MaxSize = new Size( 21, 18 );

      for( int i = 0; i < 10; i++ )
        m_listDemoMenu.Items.Add( new ListItem( "Demo List Item " + i ) );

      //Setting a custom border.
      m_listDemoMenu.BorderChars = new[] { '+', '+', '+', '+', '-', '|' };
    }

    private static void SetupSumMenu()
    {
      m_sumMenu = new Menu( "Sum Menu" );

      m_sumMenu.Items.Add( new ListItem( "Press Left or Right to change values." ) );
      m_sumMenu.Items.Add( new ListItem( "" ) );

      First = new SumItem( "First Number: " );
      First.Value = 2;
      First.onKeyPress += SumItem_onKeyPress;
      m_sumMenu.Items.Add( First );

      Second = new SumItem( "Second Number: " );
      Second.Value = 3;
      Second.onKeyPress += SumItem_onKeyPress;
      m_sumMenu.Items.Add( Second );

      m_sumMenu.Items.Add( new ListItem( "---" ) );

      Sum = new SumItem( "Sum: " );
      Sum.Value = First.Value + Second.Value;
      m_sumMenu.Items.Add( Sum );

      //Setting the indices of the menu to skip over.
      m_sumMenu.BannedIndices.AddRange( new[] { 0, 1, 4, 5 } );

      //We'll set a custom selection indicator for this menu.
      m_sumMenu.SelectionIndicator = '>';
    }

    private static void SetupMainMenu()
    {
      //For the items in this menu, we'll specify the method to handle button presses  
      //for it.
      m_mainMenu = new Menu( "Main Menu" );
      m_mainMenu.Items.Add( new ListItem( "List Demo", MainMenu_ListDemo_Click ) );
      m_mainMenu.Items.Add( new ListItem( "Sum Menu", MainMenu_Sum_Click ) );
      m_mainMenu.Items.Add( new ListItem( "Scrolling List Demo", MainMenu_Scrolling_Click ) );

      m_mainMenu.InvertedIndicator = true;
      m_mainMenu.Foreground = ConsoleColor.White;
    }

    private static void MainMenu_Scrolling_Click( Menu sender, onKeyPressArgs args )
    {
      if( args.Key.Key == ConsoleKey.Enter )
      {
        //Because the window may be resized when showing the next menu,
        //the main menu should force a resize on the next cycle.
        sender.ForceReset = true;
        m_scrollingDemo.Show();
      }
    }

    private static void MainMenu_Sum_Click( Menu sender, onKeyPressArgs args )
    {
      if( args.Key.Key == ConsoleKey.Enter )
      {
        //Because the window may be resized when showing the next menu,
        //the main menu should force a resize on the next cycle.
        sender.ForceReset = true;
        m_sumMenu.Show();
      }
    }

    private static void MainMenu_ListDemo_Click( Menu sender, onKeyPressArgs args )
    {
      if( args.Key.Key == ConsoleKey.Enter )
      {
        //Because the window may be resized when showing the next menu,
        //the main menu should force a resize on the next cycle.
        sender.ForceReset = true;
        m_listDemoMenu.Show();
      }
    }

    private static void SumItem_onKeyPress( Menu sender, onKeyPressArgs args )
    {
      //First we check if the selected index is one we want to handle.
      if( sender.SelectedIndex != 2 &&
           sender.SelectedIndex != 3 )
        return;

      //Next we get the current menu item.
      SumItem cur = (SumItem)args.MenuItem;

      //Now we check if the input key is one we want to handle.
      if( args.Key.Key != ConsoleKey.LeftArrow &&
           args.Key.Key != ConsoleKey.RightArrow )
        return;

      //Now we handle the input.
      if( args.Key.Key == ConsoleKey.LeftArrow )
        cur.Value--;

      if( args.Key.Key == ConsoleKey.RightArrow )
        cur.Value++;

      Sum.Value = First.Value + Second.Value;
    }
  }

  public class SumItem : MenuItem
  {
    public string Text
    {
      get;
      set;
    }
    public int Value
    {
      get;
      set;
    }

    public SumItem( string text )
    {
      Text = text;
    }

    public override string ToString()
    {
      return Text + Value;
    }
  }

  public class ListItem : MenuItem
  {
    public string Name
    {
      get;
      set;
    }

    public ListItem( string name )
    {
      Name = name;
    }

    public ListItem()
    {
    }

    public ListItem( string name, MenuItemClickedEventHandler clickEvent )
    {
      Name = name;
      onKeyPress += clickEvent;
    }

    public override string ToString()
    {
      return Name;
    }
  }
}
