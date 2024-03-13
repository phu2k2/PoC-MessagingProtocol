import "./App.css";
import React, { useState } from "react";
import "@fontsource/roboto/500.css";
import { Divider, Stack } from "@mui/material";
import Connection from "./components/Connection/Connection";
import ChatRoom from "./components/ChatRoom/ChatRoom";
import DataContext from "./DataContext";

function App() {
  const [user, setUser] = useState("");
  const [chatroomLoaded, setChatRoomLoaded] = useState(false);
  return (
    <Stack
      className="App"
      style={{
        position: "relative",
        top: 0,
        left: 0,
        width: "100%",
        height: "100%",
      }}
    >
      <DataContext.Provider value={{ user, setUser }}>
        <Stack sx={{ display: "flex", height: "99vh", width: "99vw" }}>
          <Stack sx={{ height: "25%" }}>
            <Connection
              chatroomLoaded={chatroomLoaded}
              setChatRoomLoaded={setChatRoomLoaded}
            ></Connection>
          </Stack>
          <Divider
            orientation="horizontal"
            flexItem
            sx={{ opacity: 1.5, borderSpacing: 1 }}
          />
          <Stack sx={{ height: "75%" }}>{chatroomLoaded && <ChatRoom />}</Stack>
        </Stack>
      </DataContext.Provider>
    </Stack>
  );
}

export default App;
