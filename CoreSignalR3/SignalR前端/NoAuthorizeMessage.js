
var hubUrl = 'http://localhost:5009/noAuthorizeHub';
//var hubUrl = 'https://botsignalr.91bihu.com/noAuthorizeHub';
//var hubUrl = 'http://192.168.5.247:5009/bihuHub';
//var hubUrl = 'http://localhost:5009/noAuthorizeHub';


var connection = new signalR.HubConnectionBuilder().withUrl(hubUrl).build();

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

connection.on("PushMessage", function (user ) {
    var li = document.createElement("li");
    console.log(user);
    li.textContent = user;
    document.getElementById("messagesList").appendChild(li);
});


connection.start().then(function(){
    connection.invoke("Join", 102).catch(function (err) {
        return console.error(err.toString());
    });
    console.log("connecteded");
}).catch(function (err) {
    return console.error(err.toString());
});

