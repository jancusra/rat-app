import Checkbox from '@mui/material/Checkbox';
import FormControlLabel from '@mui/material/FormControlLabel';
import { FormControlState } from './types';

function RatCheckbox(props: CheckboxProps) {
    function onChange(e: React.ChangeEvent<HTMLInputElement>) {
        props.callback({
            name: e.target.name,
            value: e.target.checked 
        });
    }

    return (
        <FormControlLabel 
            control={
                <Checkbox
                    name={props.name}
                    checked={props.value}
                    onChange={onChange} />
            } label={props.label} />
    );
}

export default RatCheckbox;

type CheckboxProps = {
    name: string;
    label: string;
    value: boolean;
    callback: (state: FormControlState) => void;
}