import { useEffect, useState } from 'react';
import axios from 'axios';
import List from '@mui/material/List';
import RatTreeMenuItem from '../components/RatTreeMenuItem';
import { TreeMenuItem } from './types';

function RatTreeMenu(props: TreeMenuProps) {
    const [menuData, setMenuData] = useState<Array<TreeMenuItem>>([]);

    function getMenuData() {
        axios.post(props.apiSource)
        .then(function (response) {
            setMenuData(response.data);
        });
    }

    useEffect(() => {
        getMenuData();
    }, [])

    return (
        <List
            sx={{ width: "100%", maxWidth: 300, bgcolor: "background.paper" }}
            component="nav"
            aria-labelledby="nested-list-subheader">
            {menuData.map((menuItem, index) => {
                return (
                    <RatTreeMenuItem key={index} menuData={menuItem} />
                );
            })}
        </List>
    );
}

export default RatTreeMenu;

type TreeMenuProps = {
    apiSource: string;
}