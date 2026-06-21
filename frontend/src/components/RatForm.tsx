import { ReactNode, useContext, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import Button from '@mui/material/Button';
import Alert from '@mui/material/Alert';
import RatLocales from '../contexts/RatLocales';
import { GetCurrentLanguageId } from '../Utils';
import { FormData, FormEntry, ValidationResult } from './types';

function RatForm(props: RatFormProps) {
    const [commonMessage, setMessage] = useState<string>("");
    const locales = useContext(RatLocales);
    const navigate = useNavigate();

    function handleSubmit(e: React.SyntheticEvent<HTMLFormElement>) {
        e.preventDefault();
        let reducedFormData: FormData = {};

        if (Array.isArray(props.formData))
        {
            props.formData.forEach(function (formEntry) {
                reducedFormData[formEntry.name] = formEntry.value;
            });
        }

        let data = props.entityName 
            ? { entityName: props.entityName, data: reducedFormData, languageId: GetCurrentLanguageId() } 
            : props.formData;

        axios.post(props.apiSource, data)
            .then(function (result) {
                if (props.formErrors && result.data.errors) {
                    props.formErrors(result.data.errors);
                } else {
                    props.formSubmit();
                }
            })
            .catch(function (error) {
                setMessage(error.response.data.ResultReason);
            });
    }

    function handleCancel() {
        navigate(-1);
    }

    return (
        <form className={`rat-form-box ${props.class ? props.class : ""}`} onSubmit={handleSubmit}>
            {props.children}
            <div className="form-final-buttons">
                <Button
                    type="submit" 
                    variant="contained">
                        {props.buttonContent}
                </Button>
                {props.showCancelButton ?
                    <Button
                        variant="contained"
                        color="error"
                        onClick={() => handleCancel()}>
                            {locales.Cancel}
                    </Button>
                    : null}
                {props.showBackButton ?
                    <Button
                        variant="outlined"
                        onClick={() => handleCancel()}>
                            {locales.Back}
                    </Button>
                    : null}
            </div>
            {commonMessage ? 
                <Alert 
                    className="rat-form-alert"
                    severity="error">
                        {commonMessage}
                </Alert> 
                : null}
        </form>
    );
}

export default RatForm;

type RatFormProps = {
    entityName?: string;
    apiSource: string;
    class: string;
    formData: FormData | Array<FormEntry>;
    buttonContent: string;
    showCancelButton?: boolean;
    showBackButton: boolean;
    children: ReactNode;
    formSubmit: () => void;
    formErrors?: (errors: Array<ValidationResult>) => void;
}