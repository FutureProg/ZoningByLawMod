import { useLocalization } from 'cs2/l10n';
import styles from './SidePanelHeader.module.scss';
import { SidePanelViews } from 'mods/types';
import classNames from 'classnames';

type SidePanelHeaderProps = {
    currentView: SidePanelViews;

    onSearchQueryChange: (text: string) => void;
    onViewChange: (newView: SidePanelViews) => void;
}

export const SidePanelHeader = (props: SidePanelHeaderProps) => {
    let { translate } = useLocalization();

    return (
        <div className={styles.view}>
            <div className={styles.title}>{translate('ZBL.ByLawPanel[Title]', "Zoning By Laws")}</div>
            <div className={styles.buttonRow}>
                <div
                    className={classNames(styles.viewButton, { [styles.selected]: props.currentView == 'bylaws' })}
                    onSelect={() => props.onViewChange('bylaws')}
                >
                    {translate('ZBL.ByLawPanel[ByLawList]', "Your ByLaws")}
                </div>
                <div
                    className={classNames(styles.viewButton, { [styles.selected]: props.currentView == 'editor' })}
                    onSelect={() => props.onViewChange('editor')}
                >
                    {translate('ZBL.ByLawPanel[Editor]', "Editor")}
                </div>
            </div>
        </div>
    )

}