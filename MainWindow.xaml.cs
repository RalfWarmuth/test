using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.ComponentModel;
// für die DLL-Anbindung mit DllImport
using System.Runtime.InteropServices;
// erstellen des Übergabe-Strings und
// validieren der Eingaben vllt?
using System.Xml;
using System.Xml.Serialization;
// Streamreader
using System.IO;

// Spielkram
using TNX.RssReader;

namespace TestGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // 
        private CDatenModell datenmodell;
        private TNX.RssReader.RssFeed rssFeed;

        private string[] nums = new string[15];
       

        public MainWindow()
        {
            InitializeComponent();
            // create data modell object            
            datenmodell = CDatenModell.getDatenModell();
            // Datenkontext setzen
            this.DataContext = datenmodell;

            rssFeed = TNX.RssReader.RssHelper.ReadFeed("http://www.heise.de/security/news/news-atom.xml");
            int i = rssFeed.Items.Count();

            int ctr = 0;
            foreach (RssItem item in rssFeed.Items)
            {
                string s = rssFeed.Title + "\n#############################\n" + item.Title + "\n-----------------------------\n" + item.Description;
                nums[ctr % (nums.Count())] = s;
                ctr += 1;
            }
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string s = "";
            // lese aus dem XML Dokument
            using (StreamReader sr = new StreamReader("test.xml"))
            {
                s = sr.ReadToEnd();
            }
            //datenmodell.TEXTBOXSTRING = s;
            
            // update datenmodell und an DLL verschicken
            datenmodell.sendToDll();

        }

        private void dudAnInt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            datenmodell.DECIMALUPDOWNVALUE =  (int)dudAnInt.Value;
            var tmp = Math.Abs(datenmodell.DECIMALUPDOWNVALUE);
            datenmodell.TEXTBOXSTRING = nums[tmp % (nums.Length-1)];
        }
    }

    class CDatenModell : INotifyPropertyChanged
    {
        // private fields
        private int anInt;
        private String aString;
        // protected stuff
        protected CDatenModell()
        {
        }
        
        // public stuff
        public event PropertyChangedEventHandler PropertyChanged;
        
        // und delphi dll aufrufen
        [DllImport(@"l68000_test.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern bool testFunction(int inInt, string inputString, ref int outBuffSize, ref string outBuff);
        
        public bool sendToDll()
        {

            const int bufferSize = 1024;
            int outBufSize = bufferSize;
            int outInt;
            String outPutBuff = new String('\x00', bufferSize);

            bool ret = testFunction(anInt, aString, ref outBufSize, ref outPutBuff);

            TEXTBOXSTRING = outPutBuff;
            return ret;
        }

        
        // Dieses Notify wird dazu benutzt, um das Binding auch in die andere Richtung zu betreiben
        // in dem es beim Setter gleich gecalled wird. Der Weg zurück ins Gui, quasi.
        private void Notify(string arg)
        {
            if(this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(arg));
            }
        }

        // properties
        public int  DECIMALUPDOWNVALUE
        {
            get { return anInt; }
            set
            {
                anInt = value;
                this.Notify("DECIMALUPDOWNVALUE");
            }
        }

        public String TEXTBOXSTRING
        {
            get { return aString; }
            set 
            { 
                aString = value;
                this.Notify("TEXTBOXSTRING");
            }
        }

        // static stuff
        public static CDatenModell getDatenModell()
        {
            return new CDatenModell();
        }
    }
}
