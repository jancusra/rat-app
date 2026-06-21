import React from 'react';
import { useContext } from 'react';
import Stack from '@mui/material/Stack';
import Button from '@mui/material/Button';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import AlertTitle from '@mui/material/AlertTitle';
import RatLocales from '../contexts/RatLocales';
import '../css/admin';

function Dashboard() {
    const [open, setOpen] = React.useState(false);
    const locales = useContext(RatLocales);

    const handleClick = () => {
        setOpen(true);
    };

    const handleClose = (e?: React.SyntheticEvent | Event, reason?: string) => {
        if (reason === 'clickaway') {
            return;
        }

        setOpen(false);
    };

    return (
        <Stack spacing={2} sx={{ width: '100%' }}>
            <Stack sx={{ width: '100%' }} spacing={2}>
                <Alert severity="error">
                    <AlertTitle>{locales.Error}</AlertTitle>
                    {locales.Message} ... <strong>{locales.CheckItOut}!</strong>
                </Alert>
                <Alert severity="warning">
                    <AlertTitle>{locales.Warning}</AlertTitle>
                    {locales.Message} ... <strong>{locales.CheckItOut}!</strong>
                </Alert>
                <Alert severity="info">
                    <AlertTitle>{locales.Info}</AlertTitle>
                    {locales.Message} ... <strong>{locales.CheckItOut}!</strong>
                </Alert>
                <Alert severity="success">
                    <AlertTitle>{locales.Error}</AlertTitle>
                    {locales.Message} ... <strong>{locales.CheckItOut}!</strong>
                </Alert>
            </Stack>
            <Button variant="outlined" onClick={handleClick}>
                {locales.Open} {locales.Success}
            </Button>
            <Snackbar open={open} anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }} 
                    autoHideDuration={6000} onClose={handleClose}>
                <Alert onClose={handleClose} severity="success" sx={{ width: '100%' }}>
                    {locales.Message} {locales.Success}!
                </Alert>
            </Snackbar>
        </Stack>
    );
}

export default Dashboard;