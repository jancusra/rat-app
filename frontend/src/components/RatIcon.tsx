
function RatIcon(props: IconProps) {
    return <span className={props.class ? `material-icons ${props.class}` : "material-icons"}>
        {props.name}
    </span>
}

export default RatIcon;

type IconProps = {
    name: string;
    class?: string;
}