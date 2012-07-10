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
    private static MenuItem First, Second, Sum;

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

      for ( int i = 0; i < 20; i++ )
        m_scrollingDemo.Items.Add( new MenuItem( "Item " + i ) );
    }

    private static void SetupListDemo()
    {
      //Because this is a list, we'll hide the selection star.
      m_listDemoMenu = new Menu( "List Demo" );
      m_listDemoMenu.ShowSelected = false;

      //We'll also set a maximum size to demonstrate the clipping.
      m_listDemoMenu.MaxSize = new Size( 21, 18 );

      for( int i = 0; i < 10; i++ )
        m_listDemoMenu.Items.Add( new MenuItem( "Demo List Item " + i ) );

      //Setting a custom border.
      m_listDemoMenu.BorderChars = new[] { '+', '+', '+', '+', '-', '|' };
    }

    private static void SetupMainMenu()
    {
      //For the items in this menu, we'll specify the method to handle button presses  
      //for it.
      m_mainMenu = new Menu( "Main Menu" );
      m_mainMenu.Items.Add( new MenuItem( "List Demo", MainMenu_StarTrek_Click ) );
      m_mainMenu.Items.Add( new MenuItem( "Sum Menu", MainMenu_Sum_Click ) );
      m_mainMenu.Items.Add( new MenuItem( "Scrolling List Demo", MainMenu_Scrolling_Click ) );
    }

    private static void MainMenu_Scrolling_Click( MenuItem item, ConsoleKeyInfo key )
    {
      m_scrollingDemo.Show();
    }

    private static void SetupSumMenu()
    {
      m_sumMenu = new Menu( "Sum Menu" );

      m_sumMenu.Items.Add( new MenuItem( "Press Left or Right to change values." ) );
      m_sumMenu.Items.Add( new MenuItem( "" ) );

      First = new MenuItem( "First Number: 2" );
      First.CustomData.Add( "value", 2 );
      m_sumMenu.Items.Add( First );

      Second = new MenuItem( "Second Number: 3" );
      Second.CustomData.Add( "value", 3 );
      m_sumMenu.Items.Add( Second );

      m_sumMenu.Items.Add( new MenuItem( "---" ) );

      Sum = new MenuItem( "Sum: 5" );
      Sum.CustomData.Add( "value", 5 );
      m_sumMenu.Items.Add( Sum );

      //Setting the indices of the menu to skip over.
      m_sumMenu.BannedIndices.AddRange( new[] { 0, 1, 4, 5 } );

      //We need to handle incrementing the first and second values.
      m_sumMenu.onOtherButton += SumMenuOnOnOtherButton;

      //We'll set a custom selection indicator for this menu.
      m_sumMenu.SelectionIndicator = '>';
    }

    private static void SumMenuOnOnOtherButton( Menu sender, onOtherButtonHandlerArgs args )
    {
      //First we check if the selected index is one we want to handle.
      if( sender.SelectedIndex != 2 &&
           sender.SelectedIndex != 3 )
        return;

      //Next we get the current menu item.
      MenuItem cur = args.MenuItem;

      //Now we check if the input key is one we want to handle.
      if( args.Key.Key != ConsoleKey.LeftArrow &&
           args.Key.Key != ConsoleKey.RightArrow )
        return;

      //Getting the stored value.
      int temp = (int)cur.CustomData["value"];

      //Now we handle the input.
      if( args.Key.Key == ConsoleKey.LeftArrow )
      {
        temp--;
        //Set the new name of the item.
        cur.Name = cur.Name.Substring( 0, cur.Name.IndexOf( ':' ) + 2 ) + temp.ToString();
        cur.CustomData["value"] = temp;
      }

      if( args.Key.Key == ConsoleKey.RightArrow )
      {
        temp++;
        //Set the new name of the item.
        cur.Name = cur.Name.Substring( 0, cur.Name.IndexOf( ':' ) + 2 ) + temp.ToString();
        cur.CustomData["value"] = temp;
      }

      temp = (int)First.CustomData["value"] + (int)Second.CustomData["value"];
      Sum.Name = Sum.Name.Substring( 0, Sum.Name.IndexOf( ':' ) + 2 ) + temp.ToString();
    }

    private static void MainMenu_Sum_Click( MenuItem item, ConsoleKeyInfo key )
    {
      m_sumMenu.Show();
    }

    //This function handles the user pressing Enter while this menu item is selected.
    //In this case, we're just showing another menu.
    private static void MainMenu_StarTrek_Click( MenuItem item, ConsoleKeyInfo key )
    {
      m_listDemoMenu.Show();
    }
  }
}
