import React from 'react';
import { Routes, Route } from 'react-router-dom';
import RatEntityList from '../components/RatEntityList';
import RatEntityForm from '../components/RatEntityForm';
import RatEntityDetail from '../components/RatEntityDetail';

const AdminLayout = React.lazy(() => import('./index'));
const Dashboard = React.lazy(() => import('./Dashboard'));
const NotFound = React.lazy(() => import('../web/NotFound'));

function RatAdminRoutes() {
    return (
      <Routes>
        <Route path="/admin" element={<AdminLayout />}>
          <Route index element={<Dashboard />} />
          <Route path="users" element={<RatEntityList entityName="User" createNew />} />
          <Route path="users/:id" element={<RatEntityForm entityName="User" />} />
          <Route path="roles" element={<RatEntityList entityName="UserRole" createNew />} />
          <Route path="roles/:id" element={<RatEntityForm entityName="UserRole" />} />
          <Route path="logs" element={<RatEntityList entityName="Log" />} />
          <Route path="logs/:id" element={<RatEntityDetail entityName="Log" />} />
          <Route path="*" element={<NotFound />} />
        </Route>
      </Routes>
    );
}

export default RatAdminRoutes;
