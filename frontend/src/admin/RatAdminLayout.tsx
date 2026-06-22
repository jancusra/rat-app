import { useContext } from 'react';
import { Outlet } from 'react-router-dom';
import RatTreeMenu from '../components/RatTreeMenu';
import RatAppContext from '../contexts/RatAppContext';
import '../css/admin.css';

function RatAdminLayout() {
    const { hiddenAdminMenu } = useContext(RatAppContext);

    return (
        <>
            {!hiddenAdminMenu
                ? <div className="admin-menu">
                    <RatTreeMenu apiSource="/menu/getmenu" />
                </div>
                : null}
            <div className="admin-content" style={hiddenAdminMenu ? { left: '0' } : {}} >
                <Outlet />
            </div>
        </>
    );
}

export default RatAdminLayout;