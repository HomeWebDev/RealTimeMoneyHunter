/// <reference path="../Scripts/jquery-1.10.2.js" />
/// <reference path="../Scripts/jquery.signalR-2.0.0.js" />



$(function () {
    body = window.document.body;

    var moveShapeHub = $.connection.moveShapeHub;

    Shapy = {
        ShapeId: -1,
        ShapeOwner: "none",
    }

    moveShapeHub.client.clientConnected = function (model) {
        Shapy = model;
        $("<div id='g'></div>").prependTo("body");
        //$newplayer = $("<div id='"+Shapy.ShapeOwner+"'></div").prependTo("body");
        //$newplayer.text(Shapy.ShapeId);
    }

    moveShapeHub.client.clientDisconnected = function (model) {
        Shapy = model
        $toremove = $("#"+Shapy.ShapeOwner);
        $toremove.remove();
    }

});