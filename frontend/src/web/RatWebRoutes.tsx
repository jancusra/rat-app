import React from 'react';
import { Routes, Route } from 'react-router-dom';
import RatPluginRoutes from '../plugins/RatPluginRoutes';

const Home = React.lazy(() => import('./RatHome'));
const Installing = React.lazy(() => import('./init/RatInstalling'));
const Login = React.lazy(() => import('./users/RatLoginForm'));
const Register = React.lazy(() => import('./users/RatRegisterForm'));
const NotFound = React.lazy(() => import('./NotFound'));

function RatWebRoutes() {
    return (
        <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/installing" element={<Installing />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            {RatPluginRoutes}
            <Route path="*" element={<NotFound />} />
        </Routes>
    );
}

export default RatWebRoutes;