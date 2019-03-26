﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    private static int padding = 10; // Padding of elements in pixels(?)
    private static int symbolLength = 100; // Size of symbol edge in pixels(?)

    [Header("Scene UI Elements")]
    [SerializeField]
    public PlayerUI player1;
    [SerializeField]
    public PlayerUI player2;
    public GameObject statusText;

    [Header("Prefab UI Elements")]
    public GameObject comboPanel;
    public GameObject[] symbolPrefabs;
    
    [System.Serializable]
    public struct PlayerUI {
        public GameObject nameText;
        public GameObject hitpointsText;
        public GameObject comboArea;
        public GameObject nextSymbolArea;
        public GameObject currentSequenceArea;
        public GameObject lastSequenceArea;
        public Symbol LastSymbol {get; set;}
        public GameObject NextSymbol {get; set;}
        public List<GameObject> CurrentSequence {get; set;}
        public List<GameObject> LastSequence {get; set;}
        public List<List<GameObject>> ComboList {get; set;}
    }

    private InputManager input;
    private GameState game;
    private float timer;

    void Start() {
        initializeUI();
        input = new InputManager();
        this.game = new GameState();
        replaceGameState(this.game);
        this.timer = 0;
    }

    void Update() {
        this.input.getInput();
        this.timer += Time.deltaTime;
        this.placeNextSymbols(this.input);
        if (this.timer >= GameVariables.TIME_TO_ANSWER) {
            this.game = GameState.nextState(this.game, this.input);
            replaceGameState(this.game);
            this.input.clear();
            timer = 0;
        }
        this.placeTime(timer);
    }

    private void initializeUI() {
        initializeFields(this.player1);
        initializeName(this.player1, "Player 1");
        initializeHitpoints(this.player1);
        player1.ComboList = initializeComboList(this.player1);
        player1.CurrentSequence = initializeSequence(this.player1.currentSequenceArea, this.player1.CurrentSequence);
        player1.LastSequence = initializeSequence(this.player1.lastSequenceArea, this.player1.LastSequence);
        initializeFields(this.player2);
        initializeName(this.player2, "Player 2");
        initializeHitpoints(this.player2);
        player2.ComboList = initializeComboList(this.player2);
        player2.CurrentSequence = initializeSequence(this.player2.currentSequenceArea, this.player2.CurrentSequence);
        player2.LastSequence = initializeSequence(this.player2.lastSequenceArea, this.player2.LastSequence);
    }

    public void placeNextSymbols(InputManager input) {
        this.player1 = placeNextSymbol(input.Player1Symbol, this.player1);
        this.player2 = placeNextSymbol(input.Player2Symbol, this.player2);
    }

    private PlayerUI placeNextSymbol(Symbol symbol, PlayerUI player) {
        if (symbol != player.LastSymbol) {
            GameObject.Destroy(player.NextSymbol);
            RectTransform pTransform = player.nextSymbolArea.GetComponent(typeof(RectTransform)) as RectTransform;
            player.NextSymbol = placeSymbol(pTransform, symbol);
            player.LastSymbol = symbol;
        }
        return player;
    }

    private void placeTime(float timer) {
        placeText(this.statusText, "Remaining\nTime\nTo Place:\n" + Mathf.Floor(GameVariables.TIME_TO_ANSWER+1-timer).ToString());
    }

    private void replaceGameState(GameState state) {
        replaceComboLists(state);
        replaceCurrentSequences(state);
        // placeLastSequences(state);
    }

    private void replaceComboLists(GameState state) {
        this.player1 = replaceComboList(state.Player1State.Combos, this.player1);
        this.player2 = replaceComboList(state.Player2State.Combos, this.player2);
    }

    private PlayerUI replaceComboList(List<Sequence> combos, PlayerUI player) {
        List<List<GameObject>> newComboList = new List<List<GameObject>>();
        for (int i = 0; i < combos.Count; i++) {
            newComboList.Add(replaceSymbols(combos[i], player.ComboList[i]));
        }
        player.ComboList = newComboList;
        return player;
    }

    private void replaceCurrentSequences(GameState state) {
        this.player1 = replaceCurrentSequence(state.Player1State, this.player1);
        this.player2 = replaceCurrentSequence(state.Player2State, this.player2);
    }

    private PlayerUI replaceCurrentSequence(PlayerState playerState, PlayerUI player) {
        Sequence oldSequence = new Sequence(playerState.CurrSequence);
        while (playerState.CurrSequence.Count != GameVariables.SEQUENCE_LENGTH) {
            oldSequence.addSymbol(Symbol.NONE);
        }
        player.CurrentSequence = replaceSymbols(oldSequence, player.CurrentSequence);
        return player;
    }

    private List<GameObject> replaceSymbols(Sequence seq, List<GameObject> symbolObjects) {
        List<GameObject> newSymbolObjects = new List<GameObject>();
        for (int i = 0; i < seq.Count; i++) {
            newSymbolObjects.Add(replaceSymbol(symbolObjects[i], seq[i]));
        }
        return newSymbolObjects;
    }

    private GameObject replaceSymbol(GameObject oldSymbolObject, Symbol s) {
        RectTransform transform = oldSymbolObject.GetComponent(typeof(RectTransform)) as RectTransform;
        GameObject newSymbolObject = GameObject.Instantiate(this.symbolPrefabs[(int)s], transform);
        newSymbolObject.name = "Symbol";
        newSymbolObject.transform.SetParent(oldSymbolObject.transform.parent);
        GameObject.Destroy(oldSymbolObject);
        return newSymbolObject;
    }

    private List<List<GameObject>> initializeComboList(PlayerUI player) {
        List<List<GameObject>> comboList = new List<List<GameObject>>();
        RectTransform pTransform = player.comboArea.GetComponent(typeof(RectTransform)) as RectTransform;
        for (int i = 0; i < GameVariables.NUM_COMBOS; i++) {
            GameObject comboPanel = GameObject.Instantiate(this.comboPanel, pTransform);
            comboPanel.name = "Combo" + i.ToString();
            RectTransform transform = comboPanel.GetComponent(typeof(RectTransform)) as RectTransform;
            transform.anchorMin = new Vector2(0.5f, (float)(GameVariables.NUM_COMBOS - i - 1)/GameVariables.NUM_COMBOS);
            transform.anchorMax = new Vector2(0.5f, 1 - (float)i/GameVariables.NUM_COMBOS);
            transform.sizeDelta = new Vector2((float)symbolLength * GameVariables.COMBO_LENGTH + (GameVariables.COMBO_LENGTH)*padding,transform.sizeDelta.y);
            comboList.Add(intitializeSymbols(GameVariables.COMBO_LENGTH, transform));
        }
        return comboList;
    }

    private List<GameObject> initializeSequence(GameObject sequenceArea, List<GameObject> sequence) {
        RectTransform pTransform = sequenceArea.GetComponent(typeof(RectTransform)) as RectTransform;
        pTransform.sizeDelta = new Vector2((float)symbolLength * GameVariables.SEQUENCE_LENGTH + (GameVariables.SEQUENCE_LENGTH)*padding, pTransform.sizeDelta.y);
        pTransform.anchoredPosition = new Vector2(-(pTransform.sizeDelta.x / 2), 0);
        return intitializeSymbols(GameVariables.SEQUENCE_LENGTH, pTransform);
    }

    private static void initializeName(PlayerUI player, string name) {
        placeText(player.nameText, name);
    }

    private static void initializeHitpoints(PlayerUI player) {
        placeText(player.hitpointsText, GameVariables.INITIAL_HITPOINTS.ToString());
    }

    private static void placeText(GameObject container, string s) {
        Text text = container.GetComponent(typeof(Text)) as Text;
        text.text = s;
    }

    private List<GameObject> intitializeSymbols(int numSymbols, RectTransform pTransform) {
        List<GameObject> symbolObjects = new List<GameObject>();
        float center = ((float)numSymbols - 1) / 2;
        for (int i = 0; i < numSymbols; i++) {
            GameObject symbol = placeSymbol(pTransform, Symbol.NONE);
            RectTransform transform = symbol.GetComponent(typeof(RectTransform)) as RectTransform;
            transform.anchoredPosition = new Vector2((i-center)*(symbolLength+padding), transform.anchoredPosition.y);
            symbolObjects.Add(symbol);
        }
        return symbolObjects;
    }

    private void initializeFields(PlayerUI player) {
        player.LastSymbol = Symbol.NONE;
        player.NextSymbol = null;
        player.ComboList = new List<List<GameObject>>();
        player.CurrentSequence = new List<GameObject>();
        player.LastSequence = null;
    }

    private GameObject placeSymbol(RectTransform pTransform, Symbol s) {
        GameObject o = GameObject.Instantiate(this.symbolPrefabs[(int)s], pTransform);
        o.name = "Symbol";
        return o;
    }

    private List<GameObject> placeSymbols(List<RectTransform> transforms, Sequence symbols) {
        List<GameObject> symbolObjects = new List<GameObject>();
        for (int i = 0; i < transforms.Count; i++) {
            symbolObjects.Add(placeSymbol(transforms[i], symbols[i]));
        }
        return symbolObjects;
    }

}
