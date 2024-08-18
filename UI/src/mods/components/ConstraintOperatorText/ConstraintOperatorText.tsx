import { ByLawItem, ByLawPropertyOperator } from "mods/types";

export default (props: ({ className?: string, item?: ByLawItem })) => {
    let text = "";
    switch (props.item?.propertyOperator) {
        case ByLawPropertyOperator.AtLeastOne:
            text = "At Least 1 Of";
            break;
        case ByLawPropertyOperator.Is:
            text = "Is";
            break;
        case ByLawPropertyOperator.IsNot:
            text = "Is Not";
            break;
        case ByLawPropertyOperator.None:
            text = "NONE";
            break;
        case ByLawPropertyOperator.OnlyOneOf:
            text = "Only One Of";
            break;
        case ByLawPropertyOperator.AtLeast:
            text = "At Least";
            break;
        case ByLawPropertyOperator.AtMost:
            text = "At Most";
        default:
            text = "";
            break;
    }
    return (
        <div className={props.className || ""}>
            {text}
        </div>
    )
}