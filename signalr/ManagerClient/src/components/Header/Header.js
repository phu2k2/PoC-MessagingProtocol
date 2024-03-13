import { Divider, IconButton, Stack, Typography } from '@mui/material';
import React, { useContext } from 'react'
import AccountCircle from "@mui/icons-material/AccountCircle";
import DataContext from '../../DataContext';
import LogoutIcon from "@mui/icons-material/Logout";

function Header() {
  const {room} = useContext(DataContext);
    return (
      <Stack
        display={"flex"}
        flexDirection={"row"}
        paddingTop={"20px"}
        paddingBottom={"20px"}
        ml="40px"
      >
        <AccountCircle sx={{ fontSize: "60px", mr: "15px" }}></AccountCircle>
        <Typography variant="h3" alignSelf="flex-end">
          {room}
        </Typography>
        <Stack sx={{ flexGrow: 1 }}></Stack>
        <Stack sx={{ justifyContent: "center" }}>
          <IconButton fontSize="70px">
            <LogoutIcon></LogoutIcon>
          </IconButton>
        </Stack>
      </Stack>
    );
}

export default Header;