import React from 'react';
import { Routes, Route } from 'react-router-dom';

const AdminLayout = React.lazy(() => import('./index'));
const Dashboard = React.lazy(() => import('./Dashboard'));
const Users = React.lazy(() => import('./users/List'));
const UsersCE = React.lazy(() => import('./users/CreateEdit'));
const Roles = React.lazy(() => import('./roles/List'));
const RolesCE = React.lazy(() => import('./roles/CreateEdit'));
const Logs = React.lazy(() => import('./logs/List'));
const LogsDetail = React.lazy(() => import('./logs/Detail'));
const NotFound = React.lazy(() => import('../web/NotFound'));

function RatAdminRoutes() {
    return (
      <Routes>
        <Route path="/admin" element={<AdminLayout />}>
          <Route index element={<Dashboard />} />
          <Route path="users" element={<Users />} />
          <Route path="users/:id" element={<UsersCE />} />
          <Route path="roles" element={<Roles />} />
          <Route path="roles/:id" element={<RolesCE />} />
          <Route path="logs" element={<Logs />} />
          <Route path="logs/:id" element={<LogsDetail />} />
          <Route path="*" element={<NotFound />} />
        </Route>
      </Routes>
    );
}

export default RatAdminRoutes;