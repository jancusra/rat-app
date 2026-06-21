import TextField from '@mui/material/TextField';
import { FormControlState } from './types';

function RatTextField(props: TextFieldProps) {
    function onChange(e: React.ChangeEvent<HTMLInputElement>) {
        props.callback({
            name: e.target.name, 
            value: e.target.value 
        });
    }

    return (
        <TextField
            className="form-input"
            variant="outlined"
            name={props.name}
            label={props.label}
            placeholder={props.label}
            value={props.value}
            error={props.error}
            helperText={props.errorMessage}
            onChange={onChange} />
    );
}

export default RatTextField;

type TextFieldProps = {
    name: string;
    label: string;
    value?: string;
    error?: boolean;
    errorMessage?: string;
    callback: (state: FormControlState) => void;
}