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
import {
    getCurrentLanguageId,
    changeStorageItemBoolState,
    isAdminLayout,
    STORAGE_LANGUAGE_ID,
    STORAGE_HIDDEN_ADMIN_MENU
} from './Utils';
import { LocaleContext, UserData, UserContext, AppContext } from './types';

// Build-time override (RAT_API_URL); otherwise target the host the page was loaded from.
axios.defaults.baseURL = __RAT_API_URL__ || `${window.location.protocol}//${window.location.hostname}:47050/api`;
axios.defaults.withCredentials = true; // send the HttpOnly auth cookie cross-origin

function RatApp() {
    const [userData, setUserData] = useState<UserData>({});
    const [userLoaded, setUserLoaded] = useState(false);
    const [locales, setLocales] = useState<LocaleContext>({});
    const [languageId, setLanguageId] = useState<number>(getCurrentLanguageId());
    const [hiddenAdminMenu, setHiddenAdminMenu] = useState<boolean>(
        localStorage.getItem(STORAGE_HIDDEN_ADMIN_MENU) === "true"
    );

    function getUserData() {
        return axios.get("/user/getCurrentUserData")
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

    function clearUserData() {
        setUserData({});
    }

    function getLocales(langId: number) {
        axios.get(`/localization/getByLanguageId?languageId=${langId}`)
            .then(function (response) {
                setLocales(response.data);
            })
            .catch(function (error) {
                console.error("Failed to load locales", error);
            });
    }

    function setLanguage(id: number) {
        localStorage.setItem(STORAGE_LANGUAGE_ID, id.toString());
        setLanguageId(id);
    }

    function toggleAdminMenu() {
        changeStorageItemBoolState(STORAGE_HIDDEN_ADMIN_MENU);
        setHiddenAdminMenu((prev) => !prev);
    }

    const userContext: UserContext = {
        data: userData,
        getUserData,
        clearUserData
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
                        <RatRoutedContent userData={userData} userLoaded={userLoaded} />
                    </BrowserRouter>
                </RatLocales.Provider>
            </RatAppContext.Provider>
        </RatUser.Provider>
    );
}

// Inside the router so the error boundary resets on navigation (resetKey = pathname).
function RatRoutedContent(props: RatLayoutProps) {
    const location = useLocation();

    return (
        <RatErrorBoundary resetKey={location.pathname}>
            <Suspense fallback={null}>
                <RatLayout userData={props.userData} userLoaded={props.userLoaded} />
            </Suspense>
        </RatErrorBoundary>
    );
}

// Inside the router so it re-renders on client-side navigation (useLocation).
function RatLayout(props: RatLayoutProps) {
    const location = useLocation();
    const navigate = useNavigate();
    const adminArea = isAdminLayout(location.pathname);
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
