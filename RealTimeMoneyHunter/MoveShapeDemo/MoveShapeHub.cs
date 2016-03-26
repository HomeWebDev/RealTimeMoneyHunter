﻿using System;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace MoveShapeDemo
{
    public class Broadcaster
    {
        private readonly static Lazy<Broadcaster> _instance =
            new Lazy<Broadcaster>(() => new Broadcaster());
        // We're going to broadcast to all clients a maximum of 25 times per second
        private readonly TimeSpan BroadcastInterval =
            TimeSpan.FromMilliseconds(40);
        private readonly IHubContext _hubContext;
        private Timer _broadcastLoop;
        private ShapeModel _model;
        private bool _modelUpdated;
        public Broadcaster()
        {
            //test to add brachs..
            //test again
            // Save our hub context so we can easily use it 
            // Faskdklas
            // to send to its connected clients
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<MoveShapeHub>();
            _model = new ShapeModel();
            _modelUpdated = false;
            // Start the broadcast loop
            _broadcastLoop = new Timer(
                BroadcastShape,
                null,
                BroadcastInterval,
                BroadcastInterval);
        }
        public void BroadcastShape(object state)
        {
            // No need to send anything if our model hasn't changed
            if (_modelUpdated)
            {
                // This is how we can access the Clients property 
                // in a static hub method or outside of the hub entirely
                _hubContext.Clients.AllExcept(_model.LastUpdatedBy).updateShape(_model);
                _modelUpdated = false;
            }
        }
        public void UserChoose(ShapeModel clientModel)
        {
            _model = clientModel;
            //_hubContext.Clients.AllExcept(_model.LastUpdatedBy).userChoose(_model);
            _hubContext.Clients.All.userChoose(_model);
        }
        public void UpdateShape(ShapeModel clientModel)
        {
            _model = clientModel;
            _modelUpdated = true;
        }

        public void GetUser(string user)
        {
            _hubContext.Clients.Client(user).getUser(user);
        }

        public static Broadcaster Instance
        {
            get
            {
                return _instance.Value;
            }
        }
    }

    public class MoveShapeHub : Hub
    {
        // Is set via the constructor on each creation
        private Broadcaster _broadcaster;
        private static readonly ConcurrentDictionary<string, object> _connections =
            new ConcurrentDictionary<string, object>();

        public MoveShapeHub()
            : this(Broadcaster.Instance)
        {
        }
        public MoveShapeHub(Broadcaster broadcaster)
        {
            _broadcaster = broadcaster;
        }
        public void UpdateModel(ShapeModel clientModel)
        {
            clientModel.LastUpdatedBy = Context.ConnectionId;
            // Update the shape model within our broadcaster
            _broadcaster.UpdateShape(clientModel);
        }

        public void GetUser()
        {
            _broadcaster.GetUser(Context.ConnectionId);
        }

        public void UserChoose(ShapeModel clientModel)
        {
            if (clientModel.ShapeOwner == null)
            {
                clientModel.LastUpdatedBy = Context.ConnectionId;
                clientModel.ShapeOwner = Context.ConnectionId;
                _broadcaster.UserChoose(clientModel);
            }
        }

        public override Task OnConnected()
        {
            _connections.TryAdd(Context.ConnectionId, null);
            return Clients.All.clientCountChanged(_connections.Count);
        }

        public override Task OnReconnected()
        {
            _connections.TryAdd(Context.ConnectionId, null);
            return Clients.All.clientCountChanged(_connections.Count);
        }

        public override Task OnDisconnected()
        {
            object value;
            _connections.TryRemove(Context.ConnectionId, out value);
            return Clients.All.clientCountChanged(_connections.Count);
        }
    }
    public class ShapeModel
    {
        // We declare Left and Top as lowercase with 
        // JsonProperty to sync the client and server models
        [JsonProperty("left")]
        public double Left { get; set; }
        [JsonProperty("top")]
        public double Top { get; set; }
        // We don't want the client to get the "LastUpdatedBy" property
        [JsonIgnore]
        public string LastUpdatedBy { get; set; }
        [JsonProperty("ShapeId")]
        public string ShapeId { get; set; }
        [JsonProperty("ShapeOwner")]
        public string ShapeOwner { get; set; }


    }

}
