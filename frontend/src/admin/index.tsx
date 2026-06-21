import { Outlet } from 'react-router-dom';
import RatTreeMenu from '../components/RatTreeMenu';
import '../css/admin';

function RatAdminLayout() {
    const hiddenAdminMenu = localStorage.getItem("hiddenAdminMenu") == "true";

    return (
        <>
            {!hiddenAdminMenu 
            ? <div className="admin-menu">
                <RatTreeMenu apiSource="/menu/getmenu" />
            </div>
            : null}
            <div className="admin-content" style={hiddenAdminMenu ? { left:'0' } : {}} >
                <Outlet />
            </div>
        </>
    );
}

export default RatAdminLayout;