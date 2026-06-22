import { lazy } from 'react';
import { Route } from 'react-router-dom';

const RatTestPage = lazy(() => import('./RatTestPage'));

const PluginRoutes = (
    <Route path="/p-test" element={<RatTestPage />} />
);

export default PluginRoutes;