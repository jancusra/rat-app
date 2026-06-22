import { Suspense, useEffect, useState } from 'react';
import { BrowserRouter } from 'react-router-dom';
import axios from 'axios';
import RatAdminRoutes from './admin/RatAdminRoutes';
import RatWebRoutes from './web/RatWebRoutes';
import RatUser from './contexts/RatUser';
import RatLocales from './contexts/RatLocales';
import RatWebHeader from './sections/RatWebHeader';
import { IsAdminLayout, GetCurrentLanguageId } from './Utils';
import { LocaleContext, UserData, UserContext } from './types';

// Build-time override (RAT_API_URL); otherwise target the host the page was loaded from.
axios.defaults.baseURL = __RAT_API_URL__ || `${window.location.protocol}//${window.location.hostname}:47050/api`;
axios.defaults.withCredentials = true; // send the HttpOnly auth cookie cross-origin

function RatApp() {
    const [userData, setUserData] = useState<UserData>({});
    const [locales, setLocales] = useState<LocaleContext>({});

    function getUserData() {
        axios.get("/user/getCurrentUserData")
            .then(function (response) {
                setUserData(response.data);
            });
    }

    function getLocales() {
        axios.get("/localization/getByLanguageId?languageId=" + GetCurrentLanguageId())
            .then(function (response) {
                setLocales(response.data);
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
                        {IsAdminLayout() ? <RatAdminRoutes /> : <RatWebRoutes />}
                    </Suspense>
                </BrowserRouter>
            </RatLocales.Provider>
        </RatUser.Provider>
    );
}

export default RatApp;