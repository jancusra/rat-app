import { Suspense, useEffect, useState } from 'react';
import { BrowserRouter, useLocation, useNavigate } from 'react-router-dom';
import axios from 'axios';
import RatAdminRoutes from './admin/RatAdminRoutes';
import RatWebRoutes from './web/RatWebRoutes';
import RatUser from './contexts/RatUser';
import RatLocales from './contexts/RatLocales';
import RatWebHeader from './sections/RatWebHeader';
import { GetCurrentLanguageId } from './Utils';
import { LocaleContext, UserData, UserContext } from './types';

// Build-time override (RAT_API_URL); otherwise target the host the page was loaded from.
axios.defaults.baseURL = __RAT_API_URL__ || `${window.location.protocol}//${window.location.hostname}:47050/api`;
axios.defaults.withCredentials = true; // send the HttpOnly auth cookie cross-origin

function RatApp() {
    const [userData, setUserData] = useState<UserData>({});
    const [userLoaded, setUserLoaded] = useState(false);
    const [locales, setLocales] = useState<LocaleContext>({});

    function getUserData() {
        axios.get("/user/getCurrentUserData")
            .then(function (response) {
                setUserData(response.data);
            })
            .catch(function (error) {
                console.error("Failed to load user data", error);
            })
            .finally(function () {
                setUserLoaded(true);
            });
    }

    function getLocales() {
        axios.get("/localization/getByLanguageId?languageId=" + GetCurrentLanguageId())
            .then(function (response) {
                setLocales(response.data);
            })
            .catch(function (error) {
                console.error("Failed to load locales", error);
            });
    }

    const userContext: UserContext = {
        data: userData,
        getUserData
    }

    useEffect(() => {
        getUserData();
        getLocales();
    }, [])

    return (
        <RatUser.Provider value={userContext}>
            <RatLocales.Provider value={locales}>
                <BrowserRouter>
                    <RatWebHeader />
                    <Suspense fallback={<div>Loading...</div>}>
                        <RatLayout userData={userData} userLoaded={userLoaded} />
                    </Suspense>
                </BrowserRouter>
            </RatLocales.Provider>
        </RatUser.Provider>
    );
}

// Inside the router so it re-renders on client-side navigation (useLocation).
function RatLayout(props: RatLayoutProps) {
    const location = useLocation();
    const navigate = useNavigate();
    const adminArea = location.pathname.startsWith("/admin");
    const blockAdmin = adminArea && props.userLoaded && !props.userData.isAdmin;

    // Keep non-admins out of the admin area entirely.
    useEffect(() => {
        if (blockAdmin) {
            navigate("/", { replace: true });
        }
    }, [blockAdmin])

    if (!adminArea) {
        return <RatWebRoutes />;
    }

    // Wait for user data, then only admins get the admin routes (others are redirected above).
    return props.userLoaded && props.userData.isAdmin ? <RatAdminRoutes /> : null;
}

type RatLayoutProps = {
    userData: UserData;
    userLoaded: boolean;
}

export default RatApp;
