import { ByLawListView } from 'mods/ByLawListView/ByLawListView';
import styles from './SidePanel.module.scss';
import { useEffect, useState } from 'react';
import { SidePanelHeader } from 'mods/SidePanelHeader/SidePanelHeader';
import { SidePanelViews } from 'mods/types';
import { ByLawEditorView } from 'mods/ByLawEditorView/ByLawEditorView';
import { useValue } from 'cs2/api';
import { selectedByLaw$ } from 'mods/bindings';
import { isNullOrEmpty } from 'cs2/utils';

export const SidePanel = () => {
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
    }, [activeByLaw])

    return (        
        <div className={styles.view}>
            <SidePanelHeader currentView={currentView} onSearchQueryChange={onSearchChange} onViewChange={onViewChange} />
            {currentView == 'bylaws'? <ByLawListView searchQuery={searchQuery} /> : <ByLawEditorView />}
        </div>
    )
}