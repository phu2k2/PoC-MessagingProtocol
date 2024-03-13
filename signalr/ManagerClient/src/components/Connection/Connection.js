import { Button, Grid, Stack, TextField, Typography } from "@mui/material";
import React, { useContext, useEffect, useState } from "react";
import DataContext from "../../DataContext";

function Connection({ chatroomLoaded, setChatRoomLoaded }) {
  const { user, setUser } = useContext(DataContext);
   const [connectionStatus, setConnectionStatus] = useState(false);


  const handleUserNameChange = (event) => {
    setUser(event.target.value);
  }

  const handleConnectBtnEvent = () => {
    setChatRoomLoaded(true);
    setConnectionStatus(true);
  };

  const handleDisconnectBtnEvent = () => {
    setUser("");
    setChatRoomLoaded(false);
    setConnectionStatus(false);
  }
  return (
    <Stack>
      <Typography variant="h4" gutterBottom mt={5} mb={7} font>
        CHAT SYSTEM
      </Typography>
      <Grid ml={50} container flexDirection="column" spacing={3}>
        <Stack flexDirection="row">
          <Grid item xs={12} sm={3}>
            <TextField
              required
              id="userName"
              name="userName"
              label="User name"
              fullWidth
              autoComplete="given-name"
              value={user}
              variant="standard"
              onChange={handleUserNameChange}
            />
          </Grid>
          {connectionStatus === false ? (
            <Button
              variant="contained"
              color="success"
              sx={{ marginLeft: "200px" }}
              disabled={!user}
              onClick={handleConnectBtnEvent}
            >
              Connect
            </Button>
          ) : (
            <Button
              variant="contained"
              color="success"
              sx={{ marginLeft: "200px" }}
              onClick={handleDisconnectBtnEvent}
            >
              Disconnect
            </Button>
          )}
        </Stack>
      </Grid>
    </Stack>
  );
}

export default Connection;
