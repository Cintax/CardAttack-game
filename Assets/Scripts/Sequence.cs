﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequence : IEnumerable {
    private List<Symbol> sequence;

    public Sequence() {
        this.sequence = new List<Symbol>();
    }

    public Sequence(List<Symbol> sequence) {
        this.sequence = sequence;
    }

    public void addSymbol(Symbol s) {
        this.sequence.Add(s);
    }

    public List<Symbol> getSequence() {
        return this.sequence;
    }

    public Symbol getSymbol(int index) {
        if (index >= this.sequence.Count) {
            return Symbol.NONE;
        }
        return this.sequence[index];
    }

    public int getLength() {
        return this.sequence.Count;
    }

    // Returns the difference of two sequences, like a set difference A - B
    public static Sequence getDifference(Sequence seq1, Sequence seq2) {
        if (seq1 == null || seq2 == null || seq1.getLength() != seq2.getLength()) {
            return null;
        }
        Sequence newSeq = new Sequence();
        for (int i = 0; i < seq1.getLength(); i++) {
            if (!seq1.getSymbol(i).Equals(seq2.getSymbol(i))) {
                newSeq.addSymbol(seq1.getSymbol(i));
            }
        }
        return newSeq;
    }

    public static bool containsSubsequence(Sequence seq, Sequence subseq) {
        if (seq == null || subseq == null || subseq.getLength() > seq.getLength()) {
            return false;
        }
        for (int i = 0; i < seq.getLength(); i++) {
            if ((seq.getLength() - i) < subseq.getLength()) {
                break;
            }
            bool contains = true;
            for (int j = 0; j < subseq.getLength(); j++) {
                if (!seq.getSymbol(i+j).Equals(subseq.getSymbol(j))) {
                    contains = false;
                    break;
                }
            }
            if (contains == true) {
                return true;
            }
        }
        return false;
    }

    public IEnumerator GetEnumerator()
    {
        return this.sequence.GetEnumerator();
    }
}
