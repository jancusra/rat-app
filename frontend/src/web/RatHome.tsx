import { useContext } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import Button from '@mui/material/Button';
import RatUser from '../contexts/RatUser';
import RatLocales from '../contexts/RatLocales';
import ratImage from '../images/rat.png';

function RatHome() {
    const navigate = useNavigate();
    const user = useContext(RatUser);
    const locales = useContext(RatLocales);

    function logout() {
        axios.post("/auth/logout")
            .then(function () {
                location.href = "/";
            });
    }

    return <div className="action-buttons">
                <img src={ratImage} className="rat-image" alt="image" />
                {!user.data.email ?
                    <>
                        <Button variant="contained" onClick={() => {navigate("/login");}}>
                            {locales.Login}
                        </Button>
                        <Button variant="outlined" onClick={() => {navigate("/register");}}>
                            {locales.Register}
                        </Button>
                    </>
                : <Button variant="contained" onClick={logout}>
                    {locales.Logout}
                  </Button>}
            </div>
}

export default RatHome;