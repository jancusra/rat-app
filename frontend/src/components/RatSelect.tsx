import { useContext, useEffect, useState } from 'react';
import FormControl from '@mui/material/FormControl';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import Select, { SelectChangeEvent } from '@mui/material/Select';
import RatLocales from '../contexts/RatLocales';
import { FormControlState, SelectOptions } from './types';

function RatSelect(props: SelectProps) {
    const [selectValue, setSelectValue] = useState<string>("");
    const locales = useContext(RatLocales);

    function onChange(e: SelectChangeEvent) {
        setSelectValue(e.target.value);

        props.callback({
            name: props.name,
            value: Number(e.target.value)
        });
    }

    useEffect(() => {
        setSelectValue(props.value == null ? "" : String(props.value));
    }, [props.value])

    // Value not in options (e.g. default 0 or removed enum member): show it so it's not silently hidden.
    const isUnknown = selectValue !== "" && !Object.keys(props.selectData).includes(selectValue);

    return (
        <FormControl fullWidth>
            <InputLabel>{props.label}</InputLabel>
            <Select
                label={props.label}
                value={selectValue}
                onChange={onChange}>
                {isUnknown &&
                    <MenuItem value={selectValue}>
                        {`(${locales.UnknownValue || "?"}: ${selectValue})`}
                    </MenuItem>}
                {
                    Object.keys(props.selectData).map((key) => (
                        <MenuItem key={key} value={key}>{props.selectData[Number(key)]}</MenuItem>
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