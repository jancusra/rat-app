import { useContext, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import RatForm from '../../components/RatForm';
import RatTextField from '../../components/RatTextField';
import RatUser from '../../contexts/RatUser';
import RatLocales from '../../contexts/RatLocales';
import { FormControlState, FormData } from '../../components/types';

function RatLoginForm() {
    const [formData, setState] = useState<FormData>({});
    const user = useContext(RatUser);
    const locales = useContext(RatLocales);
    const navigate = useNavigate();

    function updateField(data: FormControlState) {
        setState({
            ...formData,
            [data.name]: data.value
        });
    }

    function formSubmit() {
        user.getUserData();
        navigate("/");
    }

    return (
        <RatForm
            class="rat-login-box"
            apiSource="/auth/authenticate"
            buttonContent={locales.Login}
            showBackButton={true}
            formData={formData}
            formSubmit={formSubmit}>
            <RatTextField
                name="email"
                label={locales.Email}
                callback={updateField} />
            <RatTextField
                name="password"
                label={locales.Password}
                callback={updateField} />
        </RatForm>
    );
}

export default RatLoginForm;