import { useEffect, useState } from 'react';
import Autocomplete from '@mui/material/Autocomplete';
import FormControl from '@mui/material/FormControl';
import TextField from '@mui/material/TextField';
import { FormControlState, SelectOptions } from './types';

function RatMultiSelect(props: MultiSelectProps) {
    const [selectedOptionValues, setSelectedOptionValues] = useState<Array<SelectOption>>([]);
    const [allOptionValues, setAllOptionValues] = useState<Array<SelectOption>>([]);

    function onChange(_e: React.SyntheticEvent, value: Array<SelectOption>) {
        setSelectedOptionValues(value);

        const ids = value.map(function (x: SelectOption) {
            return x.id;
        });

        if (props.callback) {
            props.callback({
                name: props.name,
                value: ids as Array<number>
            });
        }
    }

    useEffect(() => {
        const allOptions: Array<SelectOption> = [];
        const selectedOptions: Array<SelectOption> = [];

        for (const key in props.selectData) {
            const id = props.stringValues ? props.selectData[key] : Number(key);
            const option: SelectOption = { id: id, name: props.selectData[key] };
            allOptions.push(option);

            if ((props.value as Array<number | string>)?.includes(id)) {
                selectedOptions.push(option);
            }
        }

        setAllOptionValues(allOptions);
        setSelectedOptionValues(selectedOptions);
    }, [props.selectData, props.value, props.stringValues])

    return (
        <FormControl fullWidth>
            <Autocomplete
                multiple
                readOnly={props.readOnly}
                disableClearable={props.readOnly}
                forcePopupIcon={!props.readOnly}
                options={allOptionValues}
                value={selectedOptionValues}
                getOptionLabel={(option) => option.name}
                isOptionEqualToValue={(option, value) => option.id === value.id}
                renderInput={(params) => (
                    <TextField
                        {...params}
                        label={props.label}
                    />
                )}
                onChange={onChange}
            />
        </FormControl>
    );
}

export default RatMultiSelect;

type MultiSelectProps = {
    name: string;
    label?: string;
    stringValues?: boolean;
    readOnly?: boolean;
    value: Array<number> | Array<string>;
    selectData: SelectOptions;
    callback?: (state: FormControlState) => void;
}

type SelectOption = {
    id: number | string;
    name: string;
}