import RatIcon from './RatIcon';

function RatBoolIcon(props: BoolIconProps) {
    return props.value
        ? <RatIcon name="task_alt" />
        : <RatIcon name="radio_button_unchecked" />;
}

export default RatBoolIcon;

type BoolIconProps = {
    value: boolean;
}
