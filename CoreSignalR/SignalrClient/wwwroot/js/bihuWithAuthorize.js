//"use strict";

var hubUrl = 'http://localhost:5009/bihuHub';
//var httpConnection = new signalR.HttpConnection(hubUrl);
//var connection = new signalR.HubConnection(httpConnection);
var loginToken="eyJhbGciOiJSUzI1NiIsImtpZCI6IjlDRDQ4NDI3QzVDMEJCMjBBQTU5NThGODcyQTFEODI2RjNCQUZEQzQiLCJ0eXAiOiJKV1QiLCJ4NXQiOiJuTlNFSjhYQXV5Q3FXVmo0Y3FIWUp2TzZfY1EifQ.eyJuYmYiOjE1NzQwNDI5MzUsImV4cCI6MTU3NTMzODkzNSwiaXNzIjoiaHR0cDovL2lkZW50aXR5LjkxYmlodS5tZSIsImF1ZCI6WyJodHRwOi8vaWRlbnRpdHkuOTFiaWh1Lm1lL3Jlc291cmNlcyIsImNhcl9idXNpbmVzcyIsImVtcGxveWVlX2NlbnRlciJdLCJjbGllbnRfaWQiOiJhcHBDbGllbnQiLCJzdWIiOiIxMDIiLCJhdXRoX3RpbWUiOjE1NzQwNDI5MzUsImlkcCI6ImxvY2FsIiwiZW1wbG95ZWVJZCI6IjEwMiIsImNvbXBJZCI6IjEwMiIsInVzZXJOYW1lIjoi6ams5bCP57-g6aG257qnIiwidXNlckFjY291bnQiOiJtYXhpYW9jdWkiLCJEZXB0SWQiOiIxIiwiSXNBZG1pbiI6IlRydWUiLCJSb2xlVHlwZSI6IjMiLCJSb2xlSWRzIjoiNTciLCJsb2dpbkNsaWVudFR5cGUiOiIyIiwibG9naW5TdGFtcCI6IjE1NzQwNDI5MzUtNTYyMDYwN2MtNGFjNS00OGE1LWE3MmYtZTZmMDUzMTA3Mjg1Iiwic2NvcGUiOlsiY2FyX2J1c2luZXNzIiwiZW1wbG95ZWVfY2VudGVyIl0sImFtciI6WyJwd2QiXX0.ml9gmWR9d6ET2r0mx-_dF_rm4faYHA3hGVMOXcCui-B7-X2qFcG0H7fJ0toa2N1snfylUpAYK0e0jJs5kKJhgvZlizq1mmy5l7Mwc7U6_G4Cv8NRaUGYgRjLv0gbzz_8vDgWjQ2ortUAApJ_h8qv3BamcV1yNDLx--Oa0sIU-7EV70914s3k4xOIPdtrs_lWv-ay5QYVp8mecDvv7uvyKql7caz4-nFXQLQkJbpUrE8YxVrj3BS8JSzPzLEsM9TfP9xhjAQwC-_eLkNz0OmC9K0wr7xQIPsY7qNVcYDKWCeDWinIOWpy8r7KideXZsvJvzPbh_NUHHNaYq9amwWrLg";

var connection = new signalR.HubConnectionBuilder().withUrl(hubUrl,{ accessTokenFactory: () => loginToken }).build();

//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;

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
    //connection.invoke("Join", "102").catch(function (err) {
    //    return console.error(err.toString());
    //});//用认证授权就不用join方法了
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