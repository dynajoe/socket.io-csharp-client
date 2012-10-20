var io = require("socket.io").listen(3000);

io.sockets.on("connection", function (socket) {
   socket.on("data", function (data) {
      console.log("Client sent: " + data);
      
      if (data) {
         socket.emit("data", data.toUpperCase(), {length: data.length });
      }
   });
});