import { useContext, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import List from '@mui/material/List';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Collapse from '@mui/material/Collapse';
import RatIcon from './RatIcon';
import RatLocales from '../contexts/RatLocales';
import { TreeMenuItem } from './types';
import '../css/admin.css';

function RatTreeMenuItem(props: TreeMenuItemProps) {
    const [open, setOpen] = useState(true);
    const locales = useContext(RatLocales);
    const navigate = useNavigate();
    const depth = props.depth ?? 0;
    const hasChildren = props.menuData.childMenuItems.length > 0;

    function handleClick(url: string) {
        if (url) {
            navigate(url);
        } else {
            setOpen(!open);
        }
    }

    return (
        <>
            <ListItemButton onClick={() => handleClick(props.menuData.url)} sx={{ pl: 2 + depth * 2 }}>
                <ListItemIcon>
                    <RatIcon name={props.menuData.icon} />
                </ListItemIcon>
                <ListItemText primary={locales[props.menuData.title]} />
                {hasChildren && <>
                    {open ? <RatIcon name="expand_less" />
                        : <RatIcon name="expand_more" />
                    }</>}
            </ListItemButton>
            {hasChildren &&
                <Collapse in={open} timeout="auto" unmountOnExit>
                    <List component="div" disablePadding>
                        {props.menuData.childMenuItems.map((menuItem) => {
                            return (
                                <RatTreeMenuItem key={menuItem.id} menuData={menuItem} depth={depth + 1} />
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
    menuData: TreeMenuItem;
    depth?: number;
}