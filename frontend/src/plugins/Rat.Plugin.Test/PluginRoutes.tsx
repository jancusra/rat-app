import React from 'react';
import { Route } from 'react-router-dom';

const RatTestPage = React.lazy(() => import('./RatTestPage'));

const PluginRoutes = (
    <Route path="/p-test" element={<RatTestPage />} />
);

export default PluginRoutes;