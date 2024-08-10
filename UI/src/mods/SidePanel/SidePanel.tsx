import { ByLawListView } from 'mods/ByLawListView/ByLawListView';
import styles from './SidePanel.module.scss';
import { useEffect, useState } from 'react';
import { SidePanelHeader } from 'mods/SidePanelHeader/SidePanelHeader';
import { SidePanelViews } from 'mods/types';
import { ByLawEditorView } from 'mods/ByLawEditorView/ByLawEditorView';
import { useValue } from 'cs2/api';
import { selectedByLaw$ } from 'mods/bindings';
import { createPortal } from 'react-dom';
import { Tooltip } from 'cs2/ui';
import { useLocalization } from 'cs2/l10n';

export const SidePanel = () => {
    let {translate} = useLocalization();
    let [currentView, setCurrentView] = useState<SidePanelViews>('bylaws');
    let [searchQuery, setSearchQuery] = useState<string | undefined>();
    let activeByLaw = useValue(selectedByLaw$);    

    let onSearchChange = (text: string) => {
        setSearchQuery(text);
    }
    let onViewChange = (newView: SidePanelViews) => {
        setCurrentView(newView);
    }

    useEffect(() => {
        if(activeByLaw.index > 0) {
            setCurrentView('editor');
        }    
    }, [activeByLaw.index])

    let sideButtons = createPortal((
        <div className={styles.sideButtons}>
            <Tooltip tooltip={translate("ZBL.Tooltip[CreateNewByLaw]", "Create A New ByLaw")} direction='right'>
                <div className={styles.sideButton}>
                    <img src="coui://uil/Standard/Plus.svg"/>
                </div>
            </Tooltip>            
        </div>
    ), document.body);

    return (        
        <div className={styles.view}>
            <SidePanelHeader currentView={currentView} onSearchQueryChange={onSearchChange} onViewChange={onViewChange} />
            {currentView == 'bylaws'? <ByLawListView searchQuery={searchQuery} /> : <ByLawEditorView />}
            {sideButtons}
        </div>
    )
}