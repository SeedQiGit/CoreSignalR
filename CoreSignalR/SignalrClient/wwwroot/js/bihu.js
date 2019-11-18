//"use strict";

var hubUrl = 'http://localhost:5009/bihuHub';
//var httpConnection = new signalR.HttpConnection(hubUrl);
//var connection = new signalR.HubConnection(httpConnection);
var connection = new signalR.HubConnectionBuilder().withUrl(hubUrl).build();

//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;

async function start() {
    try {
        await connection.start();
        console.log("connected");
    } catch (err) {
        console.log(err);
        setTimeout(() => start(), 5000);
    }
};

connection.onclose(async () => {
    await start();
});



connection.on("ReceiveMessage", function (user, message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = user + " says " + msg;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
});

connection.on("QuoteResultMessage", function (user ) {
    var li = document.createElement("li");
    li.textContent = user;
    document.getElementById("messagesList").appendChild(li);
});

connection.start().then(function(){
    document.getElementById("sendButton").disabled = false;
    connection.invoke("Join", "102").catch(function (err) {
        return console.error(err.toString());
    });
    console.log("built connection");
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    connection.invoke("SendMessage", user, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});