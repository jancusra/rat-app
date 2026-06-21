import { useContext, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import List from '@mui/material/List';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Collapse from '@mui/material/Collapse';
import RatIcon from '../components/RatIcon';
import RatLocales from '../contexts/RatLocales';
import { TreeMenuItem } from './types';
import '../css/admin';

function RatTreeMenuItem(props: TreeMenuItemProps) {
    const [open, setOpen] = useState(true);
    const locales = useContext(RatLocales);
    const navigate = useNavigate();

    function handleClick (url: string) {
        if (url) {
            navigate(url);
        } else {
            setOpen(!open);
        }
    }

    return (
    <>
        <ListItemButton onClick={() => handleClick(props.menuData.url)}>
            <ListItemIcon>
                <RatIcon name={props.menuData.icon} />
            </ListItemIcon>
            <ListItemText primary={locales[props.menuData.title]} />
            {props.menuData.childMenuItems.length > 0 && <>
                {open ? <RatIcon name="expand_less" />
                    : <RatIcon name="expand_more" />
            }</>}
        </ListItemButton>
        {props.menuData.childMenuItems.length > 0 &&
            <Collapse in={open} timeout="auto" unmountOnExit>
                <List component="div" disablePadding>
                    {props.menuData.childMenuItems.map((menuItem, index) => {
                        return (
                            <ListItemButton key={index} onClick={() => handleClick(menuItem.url)} sx={{ pl: 4 }}>
                                <ListItemIcon>
                                    <RatIcon name={menuItem.icon} />
                                </ListItemIcon>
                                <ListItemText primary={locales[menuItem.title]} />
                            </ListItemButton>
                        );
                    })}
                </List>
            </Collapse>
        }
    </>
    );
}

export default RatTreeMenuItem;

type TreeMenuItemProps = {
    menuData: TreeMenuItem
}