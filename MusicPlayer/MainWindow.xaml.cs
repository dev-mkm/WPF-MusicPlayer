using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO;
using Microsoft.Win32;
using System.Drawing;

namespace Atena
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MediaPlayer media;
        private bool dragStarted = false;
        TimeSpan _position;
        DispatcherTimer _timer = new DispatcherTimer();
        string[] json = new string[400];
        int queue = 0;
        int playlist = 0;
        string titl = "Please Choose a Music to Start";
        int pointer = 0;
        private bool shuffle = false;
        string[] queuelist = new string[400];
        public MainWindow()
        {
            InitializeComponent();
            media = new MediaPlayer();
            media.MediaFailed += (o, args) =>
            {
                MessageBox.Show("Media Failed!!");
            };
            media.MediaOpened += (o, args) =>
            {
                media.Position = new TimeSpan(0, 0, 0, 0);
                TagLib.File tagFile;
                if (shuffle == false)
                {
                    tagFile = TagLib.File.Create(json[queue]);
                }
                else
                {
                    tagFile = TagLib.File.Create(queuelist[queue]);
                }
                Title.Text = tagFile.Tag.Title;
                Author.Text = tagFile.Tag.FirstArtist;
                titl = tagFile.Tag.Title;
                var mStream = new MemoryStream();
                var firstPicture = tagFile.Tag.Pictures.FirstOrDefault();
                if (firstPicture != null)
                {
                    MemoryStream ms = new MemoryStream(firstPicture.Data.Data);
                    ms.Seek(0, SeekOrigin.Begin);

                    // ImageSource for System.Windows.Controls.Image
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    Image.Source = bitmap;
                } else
                {
                    Image.Source = null;
                }
                pointer = 0;
                _position = media.NaturalDuration.TimeSpan;
                sliderSeek.Minimum = 0;
                sliderSeek.Maximum = _position.TotalSeconds;
                slide.Minimum = 0;
                slide.Maximum = 10;
                slide.Value = media.Volume * 10;
            };
            media.MediaEnded += (o, args) =>
            {
                if (queue < playlist)
                {
                    queue++;
                } else
                {
                    queue = 0;
                }
                if (shuffle == false)
                {
                    media.Open(new Uri(json[queue], UriKind.RelativeOrAbsolute));
                }
                else
                {
                    media.Open(new Uri(queuelist[queue], UriKind.RelativeOrAbsolute));
                }
                PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                Play.Click -= Play_Click;
                Play.Click += Pause_Click;
                sliderSeek.Value = 0;
                media.Play();
            };
            _timer.Interval = TimeSpan.FromMilliseconds(1000);
            _timer.Tick += (o, args) =>
            {
                sliderSeek.Value = media.Position.TotalSeconds;
                slide.Value = media.Volume * 10;
                if(titl != null && titl != "")
                {
                    if (titl.Length > 40)
                    {
                        if (pointer + 39 < titl.Length)
                        {
                            Title.Text = titl.Substring(pointer, 40);
                            pointer++;
                        }
                        else
                        {
                            pointer = 0;
                        }
                    }
                }
            };
            _timer.Start();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
            Play.Click -= Play_Click;
            Play.Click += Pause_Click;
            media.Play();
        }

        private void Pre_Click(object sender, RoutedEventArgs e)
        {
            sliderSeek.Value = 0;
            if (sliderSeek.Value < 10)
            {
                if (queue - 1 >= 0)
                {
                    queue--;
                }
                else
                {
                    queue = 0;
                }
                if (shuffle == false)
                {
                    media.Open(new Uri(json[queue], UriKind.RelativeOrAbsolute));
                }
                else
                {
                    media.Open(new Uri(queuelist[queue], UriKind.RelativeOrAbsolute));
                }
                PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                Play.Click -= Play_Click;
                Play.Click += Pause_Click;
                media.Play();
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            sliderSeek.Value = 0;
            if (queue + 1 < playlist)
            {
                queue++;
            }
            else
            {
                queue = 0;
            }
            if (shuffle == false)
            {
                media.Open(new Uri(json[queue], UriKind.RelativeOrAbsolute));
            }
            else
            {
                media.Open(new Uri(queuelist[queue], UriKind.RelativeOrAbsolute));
            }
            PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
            Play.Click -= Play_Click;
            Play.Click += Pause_Click;
            media.Play();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
            Play.Click += Play_Click;
            Play.Click -= Pause_Click;
            media.Pause();
        }

        private void sliderSeek_MouseLeftButtonUp(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            media.Volume = slide.Value / 10;
        }
        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key == Key.Up)
            {
                slide.Value += 1;
            }
            else if (e.Key == Key.Down)
            {
                slide.Value -= 1;
            }
            else if (e.Key == Key.Right)
            {
                sliderSeek.Value += 5;
            }
            else if (e.Key == Key.Left)
            {
                sliderSeek.Value -= 5;
            }
            else if (e.Key == Key.Space)
            {
                if (PlayIcon.Kind == MaterialDesignThemes.Wpf.PackIconKind.Play)
                {
                    PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                    Play.Click -= Play_Click;
                    Play.Click += Pause_Click;
                    media.Play();
                } else
                {
                    PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                    Play.Click += Play_Click;
                    Play.Click -= Pause_Click;
                    media.Pause();
                }
            }
        }

        private void musicMoved(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (media.Position.TotalSeconds - sliderSeek.Value > 2 || media.Position.TotalSeconds - sliderSeek.Value < -2)
            {
                media.Position = new TimeSpan(0, 0, 0, Convert.ToInt32(sliderSeek.Value));
            }
        }

        private void close(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void open(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".mp3";
            dlg.CheckFileExists = true;
            dlg.Filter = "MP3 Files (*.mp3)|*.mp3";
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                int i = 0;
                playlist = 0;
                foreach (string file in Directory.EnumerateFiles(new FileInfo(filename).DirectoryName, "*.mp3"))
                {
                    if (file == filename)
                    {
                        queue = i;
                    }
                    json[i] = file;
                    i++;
                    playlist++;
                }
                media.Open(new Uri(json[queue], UriKind.RelativeOrAbsolute));
                PlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                Play.Click -= Play_Click;
                Play.Click += Pause_Click;
                media.Play();
                ShuffleIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ShuffleDisabled;
                shuffle = false;
            }
        }

        private void Shuffle_Click(object sender, RoutedEventArgs e)
        {
            if ( shuffle == false )
            {
                int i = 0;
                Random rnd = new Random();
                string[] MyRandomArray = json.OrderBy(x => rnd.Next()).ToArray();
                bool qu = false;
                foreach (string item in MyRandomArray)
                {
                    if (item != null && item != "")
                    {
                        queuelist[i] = item;
                        if (json[queue] == item && qu == false)
                        {
                            queue = i;
                            qu = true;
                        }
                        i++;
                    }
                }
                ShuffleIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Shuffle;
                shuffle = true;
            }
            else
            {
                ShuffleIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ShuffleDisabled;
                shuffle = false;
                bool qu = false;
                int i = 0;
                foreach (string item in json)
                {
                    if (item != null && item != "")
                    {
                        if (queuelist[queue] == item && qu == false)
                        {
                            queue = i;
                            qu = true;
                        }
                        i++;
                    }
                }
            }
        }
    }
}
