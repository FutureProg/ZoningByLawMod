import { Tooltip } from 'cs2/ui';
import styles from './CounterTile.module.scss';
import { CSSProperties } from 'react';
import classNames from 'classnames';

export const CounterTile = ({thresholds = {danger: 2, warning: 20}, ...props}: {
    value: number,
    iconSrc?: string,
    hint?: string,
    className?: string;
    thresholds?: { // if less than x, then change the colour
        danger: number,
        warning: number
    }
}) => {
    const classes = classNames(styles.view,{
        [props.className ?? 'null']: props.className != undefined
    });
    const isDanger = props.value <= thresholds.danger;
    const isWarning = !isDanger && props.value <= thresholds.warning;
    const status = isDanger ? 'danger' : ( isWarning ? 'warning': 'good');
    return (
        <Tooltip tooltip={props.hint} disabled={!props.hint}>
            <div className={classes} data-status={status}>
                {props.iconSrc? <img src={props.iconSrc} /> : null }
                <div>{props.value}</div>
            </div>
        </Tooltip>        
    )

}