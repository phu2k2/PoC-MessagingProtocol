import {
  Avatar,
  Button,
  Fab,
  FormControl,
  IconButton,
  Input,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  OutlinedInput,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import React, { useContext, useEffect, useRef, useState } from "react";
import Divider from "@mui/material/Divider";
import "@fontsource/roboto/500.css";
import AddIcon from "@mui/icons-material/Add";
import SendIcon from "@mui/icons-material/Send";
import InsertCommentIcon from "@mui/icons-material/InsertComment";
import Header from "../Header/Header";
import DataContext from "../../DataContext";
import { HubConnectionBuilder } from "@microsoft/signalr";

function ChatRoom() {
  const { user } = useContext(DataContext);
  const [totalMessages, setTotalMessages] = useState([]);
  const [messages, setMessages] = useState([]);
  const [messageInput, setMessageInput] = useState("");
  const [room, setRoom] = useState(null);
  const [selectedIndex, setSelectedIndex] = useState(-1);
  const [listItems, setListItems] = useState([]);
  const [timeToClick, setTimeToClick] = useState([]);
  const [messagesLoad, setMessagesLoad] = useState(false);
  const connectionRef = useRef(null);

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl("https://localhost:7251/chathub")
      .build();

    connectionRef.current = connection;

    // connection.start().catch((err) => console.error(err.toString()));

    const startConnection = async () => {
      try {
        await connection.start();
        console.log("Connected to SignalR server");
      } catch (err) {
        console.error("Connection error:", err);
        setTimeout(startConnection, 5000); // Reconnect after 5 seconds
      }
    };

    startConnection();

    connection.on(
      "ReceiveMessage",
      (userName, message, receivedRoom, status) => {
        setTotalMessages((prevMessages) => [
          ...prevMessages,
          {
            user: userName,
            room: receivedRoom,
            message: message,
            status: status,
          },
        ]);
      }
    );

    return () => {
      connection.stop().catch((err) => console.error(err));
    };

    // connection.on("ConnectedUser", (users) => {
    //   setConnectedUsers(users);
    // });
  }, []);

  useEffect(() => {
    const currentFilteredMessages = totalMessages.filter(
      (message) => message.room === room && message.status === true
    );

    setMessages(currentFilteredMessages);
  }, [room, totalMessages]);

  useEffect(() => {
    if (room) {
      let hasReceiveMessage = true;

      // Check time to Click in Room Item
      if (timeToClick.filter((item) => item.room === room).length > 0) {
        hasReceiveMessage = false;
      }

      // Check if both user and room are available before joining
      const userConnection = {
        User: user,
        Room: room,
      };

      connectionRef.current
        .invoke("JoinRoom", userConnection, hasReceiveMessage)
        .catch((err) => console.error(err.toString()));
    }
  }, [room]);

  const handleListItemClick = (event, index) => {
    setSelectedIndex(index);
    setRoom(listItems[index].label);
    setTimeToClick((prevClick) => [
      ...prevClick,
      {
        room: room,
        timeToClick: 1,
      },
    ]);
    setMessagesLoad(true);
  };

  const handleClickAddBtn = () => {
    const roomName = document.getElementById("inputRoom").value;

    const hasLabel = listItems.some((item) => item.label === roomName);

    if (!hasLabel) {
      const newListItem = { label: roomName };
      setListItems([...listItems, newListItem]);
    } else {
      console.log("Duplicate Item");
    }
  };

  const handleClickDeleteBtn = (index) => {
    const updatedList = listItems.filter((_, idx) => idx !== index);

    setListItems(updatedList);
  };

  const handleClickSendBtn = (event) => {
    connectionRef.current
      .invoke("SendMessage", messageInput, room, true)
      .catch((err) => console.error(err.toString()));
  };

  return (
    <DataContext.Provider value={{ room, setRoom }}>
      <Stack
        sx={{
          display: "flex",
          flexDirection: "row",
          height: "99vh",
          width: "99vw",
        }}
      >
        <Stack width="20%" sx={{ backgroundColor: "#FFFF", marginTop: "20px" }}>
          <Stack display={"flex"} justifyContent={"space-between"}>
            <Typography variant="h6" mb="12px">
              Chats
            </Typography>
            <Stack
              display="flex"
              flexDirection={"row"}
              justifyContent={"center"}
            >
              <Input id="inputRoom" />
              <IconButton
                sx={{ backgroundColor: "#E0E0E0" }}
                onClick={handleClickAddBtn}
              >
                <AddIcon></AddIcon>
              </IconButton>
            </Stack>
          </Stack>
          <Typography
            sx={{
              alignSelf: "baseline",
              color: "#777C81",
              mt: 4,
              ml: 2,
            }}
            component="div"
          >
            Rooms
          </Typography>
          <List component="nav" aria-label="main mailbox folders">
            {listItems.map((item, index) => (
              <ListItemButton
                key={index}
                selected={selectedIndex === index}
                onClick={(event) => handleListItemClick(event, index)}
              >
                <ListItemIcon>
                  <InsertCommentIcon />
                </ListItemIcon>
                <ListItemText primary={item.label} />
              </ListItemButton>
            ))}
          </List>
        </Stack>
        <Divider
          orientation="vertical"
          flexItem
          sx={{ opacity: 1.5, borderSpacing: 1 }}
        />
        {messagesLoad ? (
          <Stack
            width="80%"
            display={"grid"}
            gridTemplateRows={"auto 1fr auto"}
            gridTemplateColumns={"1fr"}
          >
            <Stack backgroundColor="#EEEE" gridRow={1} gridColumn={1}>
              <Header></Header>
              <Divider
                orientation="horizontal"
                flexItem
                sx={{ opacity: 1.5, borderSpacing: 1 }}
              />
            </Stack>

            <Stack
              gridRow={2}
              gridColumn={1}
              sx={{ maxHeight: "565px", overflowY: "auto" }}
            >
              <List>
                {messages.map((message, index) => (
                  <ListItem
                    key={index}
                    sx={{
                      display: "flex",
                      mb: 2,
                      justifyContent:
                        message.user === "JoinRoomNotification"
                          ? "space-around"
                          : user === message.user
                          ? "flex-end"
                          : "flex-start",
                    }}
                  >
                    {user === message.user ? (
                      <Stack flexDirection={"row"} alignItems={"center"}>
                        <FormControl sx={{ width: "27ch" }}>
                          <OutlinedInput
                            value={message.message}
                            multiline
                            inputProps={{
                              readOnly: true,
                            }}
                            sx={{
                              width: "100%",
                              fontSize: 16,
                              backgroundColor: "#E0E0E0",
                            }}
                          />
                        </FormControl>
                        <Avatar sx={{ ml: 2 }}>
                          {message.user.charAt(0).toUpperCase()}
                        </Avatar>
                      </Stack>
                    ) : (
                      <Stack>
                        {message.user !== "JoinRoomNotification" ? (
                          <Stack flexDirection={"row"}>
                            <Avatar sx={{ mr: 2 }}>
                              {message.user.charAt(0).toUpperCase()}
                            </Avatar>
                            <FormControl sx={{ width: "30ch" }}>
                              <OutlinedInput
                                backgroundColor="#E0E0E0"
                                value={message.message}
                                multiline
                                inputProps={{
                                  readOnly: true,
                                  disableUnderline: true,
                                }}
                                sx={{
                                  borderColor: "rgba(0, 0, 0, 0.1)",
                                  border: "0px",
                                  width: "100%",
                                  fontSize: 16,
                                  backgroundColor: "#E0E0E0",
                                }}
                              />
                            </FormControl>
                          </Stack>
                        ) : (
                          <Stack>
                            <Fab variant="extended">{message.message}</Fab>
                          </Stack>
                        )}
                      </Stack>
                    )}
                  </ListItem>
                ))}
              </List>
            </Stack>
            <Stack
              gridRow={3}
              gridColumn={1}
              display={"flex"}
              flexDirection={"row"}
            >
              <Stack width="88%">
                <TextField
                  id="messageInput"
                  value={messageInput}
                  onChange={(e) => setMessageInput(e.target.value)}
                ></TextField>
              </Stack>
              <Stack width="10%">
                <Button
                  variant="contained"
                  color="success"
                  sx={{ marginLeft: "10px" }}
                  onClick={handleClickSendBtn}
                >
                  <SendIcon></SendIcon>
                </Button>
              </Stack>
            </Stack>
          </Stack>
        ) : null}
      </Stack>
    </DataContext.Provider>
  );
}

export default ChatRoom;
