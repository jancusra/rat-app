import { useEffect, useState } from 'react';
import FormControl from '@mui/material/FormControl';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import Select, { SelectChangeEvent } from '@mui/material/Select';
import { FormControlState, SelectOptions } from './types';

function RatSelect(props: SelectProps) {
    const [selectValue, setSelectValue] = useState<string>("");

    function onChange(e: SelectChangeEvent) {
        setSelectValue(e.target.value);

        props.callback({
            name: props.name,
            value: parseInt(e.target.value)
        });
    }

    useEffect(() => {
        setSelectValue(String(props.value));
    }, [])

    return (
        <FormControl fullWidth>
            <InputLabel>{props.label}</InputLabel>
            <Select
                label={props.label}
                value={selectValue}
                onChange={onChange}>
                {
                    Object.keys(props.selectData).map((key, index) => ( 
                        <MenuItem key={index} value={key}>{props.selectData[key]}</MenuItem>
                    ))
                }
            </Select>
        </FormControl>
    );
}

export default RatSelect;

type SelectProps = {
    name: string;
    label: string;
    value: number;
    selectData: SelectOptions;
    callback: (state: FormControlState) => void;
}