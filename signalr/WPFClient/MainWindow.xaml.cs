using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.PortableExecutable;
using System.Reflection;
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
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks.Dataflow;

namespace WPFClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // HubConnection instance
        HubConnection connection;

        // List of total messages that this user has received of all rooms
        List<Message> totalMessages = new List<Message>();

        // List of messages that this user has received of the selected room
        List<Message> messages = new List<Message>();

        public MainWindow()
        {
            InitializeComponent();

            // Create a new instance of HubConnection
            connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7251/chathub")
                .WithAutomaticReconnect()
                .Build();

            // Handle the "ReceiveMessage" event from the server
            connection.On<string, string, string, bool>("ReceiveMessage", (user, message, receivedRoom, status) =>
            {
                try
                {
                    // Update the UI on the main thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Create a new Message object
                        var newMessage = new Message
                        {
                            User = user,
                            Room = receivedRoom,
                            MessageText = message,
                            Status = status,
                        };
                        // Add the new message to the totalMessages list
                        totalMessages.Add(newMessage);

                        // Update the message component for viewing the detail;
                        messageComponent_Change(Header.Children.OfType<Label>().First().Content.ToString());

                        // Print the message details to the console
                        Console.WriteLine("-------------MESSAGE-WPFCLIENT------------");
                        Console.WriteLine($"User send the message: {newMessage.User}");
                        Console.WriteLine($"Room: {newMessage.Room}");
                        Console.WriteLine($"Message: {newMessage.MessageText}");
                        Console.WriteLine($"Status: {newMessage.Status}");
                        Console.WriteLine("-------------------------------------");
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            });

            // Handle the "Reconnecting" event
            connection.Reconnecting += async (sender) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    Console.WriteLine("Attempting to reconnect...");
                });

                await Task.CompletedTask;
            };

            // Handle the "Reconnected" event
            connection.Reconnected += async (sender) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    Console.WriteLine("Reconnected to the server");
                });

                await Task.CompletedTask;
            };

            // Handle the "Closed" event
            connection.Closed += async (sender) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    signinButton.IsEnabled = true;
                });

                await Task.CompletedTask;
            };
        }

        // Handle the "OpenConnection" button click event
        private async void OpenConnection_Click(object sender, RoutedEventArgs e)
        {
            if (chatServicePage.Visibility == Visibility.Hidden)
            {
                userInput.IsReadOnly = true;
                signinButton.Content = "Close Connection";
                chatServicePage.Visibility = Visibility.Visible;

                Console.WriteLine("--------WPFCLIENT-CONNECTION-NOTIFICATION--------");
                Console.WriteLine($"{userInput.Text} have connected to Server");
                Console.WriteLine("--------------------------------");

                try
                {
                    // Start the connection to the server
                    await connection.StartAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Connection failed!:{ex.Message}");
                    MessageBox.Show("Connection failed!");
                }
            }
            else
            {
                signinButton.Content = "Sign In";
                userInput.IsReadOnly = false;
                chatServicePage.Visibility = Visibility.Hidden;
            }
        }

        // Handle the "AddRoom" button click event
        private void AddRoom_Click(object sender, RoutedEventArgs e)
        {
            int count = 0;

            // Check if the room name already exists
            foreach (Button button in roomList.Children.OfType<Button>())
            {
                if (button.Content.ToString() == roomInput .Text)
                {
                    count++;
                }
            }

            if (count == 0)
            {
                string roomName = roomInput.Text;
                Button newButton = new Button();
                newButton.Content = roomName;
                newButton.Click += RoomItem_Click;
                newButton.Height = 34;
                newButton.FontSize = 16;
                newButton.Background = Brushes.Transparent;
                newButton.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB7B7B7"));
                roomList.Children.Add(newButton);

                // After adding the room, reset the room input
                roomInput.Text = "";
            }
            else
            {
                MessageBox.Show("Room name already exists");
            }
        }

        // Handle the "RoomItem" button click event
       private async void RoomItem_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            if (button.Background == Brushes.Transparent)
            {
                // Handle UI Event for Room Item
                foreach (Button otherButton in roomList.Children.OfType<Button>())
                {
                    if (otherButton != button)
                    {
                        otherButton.Background = Brushes.Transparent;
                    }
                }
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB7B7B7"));

                Header.Children.OfType<Label>().First().Content = button.Content;

                // Filter the messages based on the selected room
                bool status = true;
                if (totalMessages.Where(m => m.Room == button.Content.ToString() && m.User == "JoinRoomNotification").ToList().Count >= 1)
                {
                    status = false;
                }

                // Invoke the "JoinRoom" method on the server
                await connection.InvokeAsync("JoinRoom", new UserRoomConnection
                {
                    User = userInput.Text,
                    Room = button.Content.ToString()
                }, status);
            }
        }

        // Update the message component based on the selected room
        private void messageComponent_Change(string room)
        {
            // After changing the room or sending new message, reset the message input
            messageRoom.Children.Clear();

            // Filter the messages based on the selected room and status
            messages = totalMessages.Where(m => m.Room == room && m.Status == true).ToList();


            // Create a new message panel for each message
            foreach (Message message in messages)
            {
                var messagePanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    MaxWidth = 350,
                    MaxHeight = 100,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var userAvatar = new Image
                {
                    Width = 27,
                    Height = 21,
                    Margin = new Thickness(10),
                    Source = new BitmapImage(new Uri("D:\\poc-msg-protocol-2\\poc-msg-protocol\\signalr\\WPFClient\\Image\\userIcon.png"))
                };

                var messageBox = new TextBox
                {
                    IsEnabled = false,
                    MaxWidth = 300,
                    FontSize = 14,
                    VerticalAlignment = VerticalAlignment.Center,
                    Background = null,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Text = message.MessageText,
                    BorderThickness = new Thickness(1.25),
                    FontWeight = FontWeights.Bold,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0x6E, 0x6F, 0x73))
                };

                if (message.User == userInput.Text)
                {
                    messagePanel.HorizontalAlignment = HorizontalAlignment.Right;
                    messagePanel.Children.Add(messageBox);
                    messagePanel.Children.Add(userAvatar);
                }
                else if (message.User == "JoinRoomNotification")
                {
                    messagePanel.HorizontalAlignment = HorizontalAlignment.Center;
                    messagePanel.Children.Add(messageBox);
                }
                else
                {
                    messagePanel.Children.Add(userAvatar);
                    messagePanel.Children.Add(messageBox);
                }
                messageRoom.Children.Add(messagePanel);
            }
        }

        // Handle the "sendMessageButton" click event
        private void sendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            // Invoke the "SendMessage" method on the server
            bool status = true;

            connection.InvokeAsync("SendMessage", messageInput.Text, Header.Children.OfType<Label>().First().Content.ToString(), status);

            // After send the message, reset message input
            messageInput.Text = "";
        }
    }
}

