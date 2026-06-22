import { useCallback, useEffect, useState } from 'react';
import axios from 'axios';
import List from '@mui/material/List';
import RatTreeMenuItem from './RatTreeMenuItem';
import { TreeMenuItem } from './types';

function RatTreeMenu(props: TreeMenuProps) {
    const [menuData, setMenuData] = useState<Array<TreeMenuItem>>([]);

    const getMenuData = useCallback(function () {
        axios.post(props.apiSource)
        .then(function (response) {
            setMenuData(response.data);
        })
        .catch(function (error) {
            console.error("Failed to load menu", error);
        });
    }, [props.apiSource]);

    useEffect(() => {
        getMenuData();
    }, [getMenuData])

    return (
        <List
            sx={{ width: "100%", maxWidth: 300, bgcolor: "background.paper" }}
            component="nav"
            aria-labelledby="nested-list-subheader">
            {menuData.map((menuItem) => {
                return (
                    <RatTreeMenuItem key={menuItem.id} menuData={menuItem} />
                );
            })}
        </List>
    );
}

export default RatTreeMenu;

type TreeMenuProps = {
    apiSource: string;
}