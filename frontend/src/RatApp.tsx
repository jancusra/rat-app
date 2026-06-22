import { Suspense, useEffect, useState } from 'react';
import { BrowserRouter, useLocation, useNavigate } from 'react-router-dom';
import axios from 'axios';
import RatAdminRoutes from './admin/RatAdminRoutes';
import RatWebRoutes from './web/RatWebRoutes';
import RatUser from './contexts/RatUser';
import RatLocales from './contexts/RatLocales';
import RatAppContext from './contexts/RatAppContext';
import RatWebHeader from './sections/RatWebHeader';
import RatErrorBoundary from './components/RatErrorBoundary';
import { GetCurrentLanguageId, ChangeStorageItemBoolState } from './Utils';
import { LocaleContext, UserData, UserContext, AppContext } from './types';

// Build-time override (RAT_API_URL); otherwise target the host the page was loaded from.
axios.defaults.baseURL = __RAT_API_URL__ || `${window.location.protocol}//${window.location.hostname}:47050/api`;
axios.defaults.withCredentials = true; // send the HttpOnly auth cookie cross-origin

function RatApp() {
    const [userData, setUserData] = useState<UserData>({});
    const [userLoaded, setUserLoaded] = useState(false);
    const [locales, setLocales] = useState<LocaleContext>({});
    const [languageId, setLanguageId] = useState<number>(GetCurrentLanguageId());
    const [hiddenAdminMenu, setHiddenAdminMenu] = useState<boolean>(
        localStorage.getItem("hiddenAdminMenu") === "true"
    );

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

    function getLocales(langId: number) {
        axios.get("/localization/getByLanguageId?languageId=" + langId)
            .then(function (response) {
                setLocales(response.data);
            })
            .catch(function (error) {
                console.error("Failed to load locales", error);
            });
    }

    function setLanguage(id: number) {
        localStorage.setItem("languageId", id.toString());
        setLanguageId(id);
    }

    function toggleAdminMenu() {
        ChangeStorageItemBoolState("hiddenAdminMenu");
        setHiddenAdminMenu((prev) => !prev);
    }

    const userContext: UserContext = {
        data: userData,
        getUserData
    }

    const appContext: AppContext = {
        languageId,
        setLanguage,
        hiddenAdminMenu,
        toggleAdminMenu
    }

    useEffect(() => {
        getUserData();
    }, [])

    // Re-fetch only locales when the language changes (no full page reload).
    useEffect(() => {
        getLocales(languageId);
    }, [languageId])

    return (
        <RatUser.Provider value={userContext}>
            <RatAppContext.Provider value={appContext}>
                <RatLocales.Provider value={locales}>
                    <BrowserRouter>
                        <RatWebHeader />
                        <RatErrorBoundary>
                            <Suspense fallback={<div>Loading...</div>}>
                                <RatLayout userData={userData} userLoaded={userLoaded} />
                            </Suspense>
                        </RatErrorBoundary>
                    </BrowserRouter>
                </RatLocales.Provider>
            </RatAppContext.Provider>
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
