import { Bounds1Field, Bounds1FieldProps } from '../Bounds1Field'
import styles from './ByLawItemBounds1Editor.module.scss'

export interface ByLawItemBounds1EditorProps extends Bounds1FieldProps { 
}

export const ByLawItemBounds1Editor = (props: ByLawItemBounds1EditorProps) => {

    return (
        <div >
            <Bounds1Field name={props.name} bounds={props.bounds} onChange={props.onChange} />
        </div>        
    )

}