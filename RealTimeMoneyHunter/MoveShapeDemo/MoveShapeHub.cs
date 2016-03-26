using System;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace MoveShapeDemo
{
    public class Broadcaster
    {
        private readonly static Lazy<Broadcaster> _instance =
            new Lazy<Broadcaster>(() => new Broadcaster());
        // We're going to broadcast to all clients a maximum of 25 times per second
        private readonly TimeSpan BroadcastInterval =
            TimeSpan.FromMilliseconds(50);
        private readonly IHubContext _hubContext;
        private Timer _broadcastLoop;
        private ShapeModel _model;
        private ShapeModel _coinModel;
        private bool _modelUpdated;
        private bool _coinUpdated;
        public Broadcaster()
        {
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

            //If coin was updated
            if (_coinUpdated)
            {
                _hubContext.Clients.All.updateCoinShape(_coinModel);
                _coinUpdated = false;
            }
        }
        public void UpdateShape(ShapeModel clientModel)
        {
            _model = clientModel;
            _modelUpdated = true;
        }
        public void UpdateCoinShape(ShapeModel coinModel)
        {
            _coinModel = coinModel;
            _coinUpdated = true;
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

        public void UpdateScore(ShapeModel clientModel)
        {
            if (System.Web.HttpContext.Current.Application["score"] == null)
            {
                System.Web.HttpContext.Current.Application["score"] = 0;
            }
            else
            {
                System.Web.HttpContext.Current.Application["score"] = Convert.ToInt32(System.Web.HttpContext.Current.Application["score"]) + 1;
            }

            int testScore = Convert.ToInt32(System.Web.HttpContext.Current.Application["score"]);

            clientModel.LastUpdatedBy = Context.ConnectionId;
            // Update the shape model within our broadcaster
            _broadcaster.UpdateShape(clientModel);
        }

        public void MoveCoin(ShapeModel coinModel)
        {
            //Move coin to random position within game area
            Random rnd = new Random();
            int randLeft = rnd.Next(1, 1000);
            int randTop = rnd.Next(1, 500);
            coinModel.Left = randLeft;
            coinModel.Top = randTop;

            // Update the shape model within our broadcaster
            _broadcaster.UpdateCoinShape(coinModel);
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
    }

}
