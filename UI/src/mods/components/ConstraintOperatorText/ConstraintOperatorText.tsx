import { useLocalization } from "cs2/l10n";
import { ByLawItem, ByLawPropertyOperator } from "mods/types";

export default (props: ({ className?: string, item?: ByLawItem })) => {
    let {translate} = useLocalization();
    let text = "";
    if (props.item) {        
        text = translate(`ZBL.PropertyOperator[${ByLawPropertyOperator[props.item!.propertyOperator]}]`, "")!;
    }    
    return (
        <div className={props.className || ""}>
            {text}
        </div>
    )
}