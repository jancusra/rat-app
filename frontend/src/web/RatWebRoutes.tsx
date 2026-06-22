import { lazy } from 'react';
import { Routes, Route } from 'react-router-dom';
import RatPluginRoutes from '../plugins/RatPluginRoutes';

const Home = lazy(() => import('./RatHome'));
const Installing = lazy(() => import('./init/RatInstalling'));
const Auth = lazy(() => import('./users/RatAuthForm'));
const NotFound = lazy(() => import('./NotFound'));

function RatWebRoutes() {
    return (
        <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/installing" element={<Installing />} />
            <Route path="/login" element={<Auth mode="login" />} />
            <Route path="/register" element={<Auth mode="register" />} />
            {RatPluginRoutes}
            <Route path="*" element={<NotFound />} />
        </Routes>
    );
}

export default RatWebRoutes;