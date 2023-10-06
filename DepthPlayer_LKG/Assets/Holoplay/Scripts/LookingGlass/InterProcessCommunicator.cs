//Copyright 2017-2021 Looking Glass Factory Inc.
//All rights reserved.
//Unauthorized copying or distribution of this file, and the source code contained herein, is strictly prohibited.

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * USE
 * Sending messages between two apps on the same computer.
 *
 * INSTRUCTIONS
 * - Add this component to an object in each app.
 * - A "Sender" communicator will send messages on the senderPort
 *   to be received in the "Receiver" communicator receiverPort,
 *   so match these port numbers.
 * - If you want two "Both" communicators to talk, likewise match
 *   the sender port number on one to the receiver port number on
 *   the other, and vice versa.
 * - Message signing is not necessary, but helps validate the data
 *
 * KNOWN ISSUE
 * On Windows using Both role, may see error
 * "existing connection was forcibly closed by the remote host".
 * Use two InterProcessCommunicators, one sender one receiver,
 * as a workaround.
 */

namespace LookingGlass {
    public class InterProcessCommunicator : MonoBehaviour
    {
        // subscribe to event to get message
        public delegate void MessageReceived( string message );
        public event MessageReceived OnMessageReceived;
    
        // communicates only with processes on same computer
        private const string LocalHostIP = "127.0.0.1";
        private const int MaxBufferSize = 65536;
    
        public enum Role
        {
            None = -1,
            Sender = 0,
            Receiver = 1,
            Both = 2,
        }

        public Role role = Role.None;
    
        // must be unique per receiver
        public int receiverPort = 8080;
        public int senderPort = 8081;
    
        // for security and identification, must be same on sender and receiver
        public bool signMessages = true;
    
        // should be uncommon and unique per app
        public char signingChar = '☆';
    
        private IPEndPoint anyIP = new IPEndPoint( IPAddress.Any, 0 );
        private Queue<string> messageQueue = new Queue<string>();
    
        private UdpClient client;
        private Thread receiveThread;
        private IPEndPoint remoteEndPoint;

        void Start()
        {
#if !UNITY_2018_4_OR_NEWER && !UNITY_2019_1_OR_NEWER
            Debug.LogError("[HoloPlay] Dual Monitior Applications require Unity version 2018.4 or newer!");
# endif
            if( client == null )
            {
                Init();
            }
        }

        void OnDestroy()
        {
            Disconnect();
        }
    
        void OnApplicationQuit()
        {
            Disconnect();
        }

        // call to send message
        public void SendData( string message )
        {
            SendData( message, -1 );
        }
    
        // call to send message
        public void SendData( string message, int port )
        {
            if( ( role != Role.Sender && role != Role.Both ) || string.IsNullOrEmpty( message ) )
                return;

            if( signMessages )
            {
                message += signingChar;
            }
        
            if( client == null )
            {
                Init();
            }

            if( client != null )
            {
                byte[] data = Encoding.UTF8.GetBytes( message );
            
                if( data.Length > client.Client.SendBufferSize )
                {
                    Debug.LogError( "Error UDP send: Message too big" );
                    return;
                }
            
                try
                {
                    client.Send( data, data.Length, port == -1 ? remoteEndPoint : CreateEndPoint( port ) );
                }
                catch( Exception e )
                {
                    Debug.LogError( "Error UDP send: " + e.Message );
                }
            }
        }

        private void ReceiveData()
        {
            if( role != Role.Receiver && role != Role.Both )
                return;

            while( client != null )
            {
                try
                {
                    byte[] data = client.Receive( ref anyIP );
                    string message = Encoding.UTF8.GetString( data );
                
                    if( !string.IsNullOrEmpty( message ) )
                    {
                        if( signMessages )
                        {
                            if( message[ message.Length - 1 ] == signingChar )
                            {
                                // remove endChar
                                messageQueue.Enqueue( message.Substring( 0, message.Length - 1 ) );
                            }
                        }
                        else
                        {
                            messageQueue.Enqueue( message );
                        }
                    }
                }
                catch( ThreadAbortException e )
                {
                    Debug.Log("Thread Abort Error: " + e.Message);
                }
                catch( Exception e )
                {
                    Debug.LogError( "Error UDP receive: " + e.Message );
                }
            }
        }

        private IPEndPoint CreateEndPoint( int port )
        {
            IPAddress ip;
            if( IPAddress.TryParse( LocalHostIP, out ip ) )
            {
                return new IPEndPoint( ip, port );
            }
            else
            {
                return new IPEndPoint( IPAddress.Broadcast, port );
            }
        }
    
        // evaluate messages on main thread
        private IEnumerator DoEvaluateMessages()
        {
            while( true )
            {
                while( messageQueue.Count > 0 )
                {
                    EvaluateMessage( messageQueue.Dequeue() );
                }
            
                yield return null;
            }
        }
    
        private void EvaluateMessage( string message )
        {
            if( OnMessageReceived != null )
                OnMessageReceived( message );
        }
    
        private void Init()
        {
            if( role == Role.None )
            {
                Debug.LogWarning( "InterProcessController role is set to None." );
                enabled = false;
                return;
            }

            remoteEndPoint = CreateEndPoint( senderPort );

            if( role == Role.Sender )
            {
                client = new UdpClient();
            }
            else if( role == Role.Receiver || role == Role.Both )
            {
                client = new UdpClient( receiverPort );
            }

            if( client != null )
            {
                client.EnableBroadcast = true;
                client.Client.ReceiveBufferSize = MaxBufferSize;
                client.Client.SendBufferSize = MaxBufferSize;

                // client.Client.IOControl((IOControlCode)(-1744830452), new byte[] { 0, 0, 0, 0 }, null);
            }

            if( role == Role.Receiver || role == Role.Both )
            {
                receiveThread = new Thread( new ThreadStart( ReceiveData ) );
                receiveThread.IsBackground = true;
                receiveThread.Start();
            
                StartCoroutine( DoEvaluateMessages() );
            }
        }
    
        private void Disconnect()
        {
            if( receiveThread != null )
            {
                receiveThread.Abort();
                receiveThread = null;
            }
        
            if( client != null )
            {
                client.Close();
                client = null;
            }
        }
    }
}