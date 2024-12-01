using System;
using System.Collections.Generic;

namespace ChadNedzlek.AdventOfCode.Library;

public static class Elfo
{
    // From the best name generator: https://www.fantasynamegenerators.com/christmas-elf-names.php
    private static readonly string[] FirstNames =
    {
        "Angel", "Belle", "Berry", "Bing", "Bling", "Blitz", "Blue", "Bluebell", "Brandy", "Brownie", "Bubbles",
        "Button", "Buttons", "Candy", "Candycane", "Carol", "Cherry", "Choco", "Cinnamon", "Clove", "Coco", "Cocoa",
        "Cookie", "Cupcake", "Dandy", "Dash", "Ember", "Emerald", "Eve", "Evie", "Faith", "Fig", "Figgy", "Fizzy",
        "Flake", "Fluffy", "Fruity", "Fudge", "Fuzzle", "Garland", "Ginger", "Gingernuts", "Gingersnap", "Glitter",
        "Gloria", "Glory", "Hazel", "Holly", "Honey", "Honeycomb", "Hope", "Ice", "Ivy", "Jangle", "Jewel",
        "Jingle", "Jolly", "Joy", "Juniper", "Merry", "Mince", "Mint", "Mistle", "Mistletoe", "Noelle", "Nutmeg",
        "Pepper", "Peppetmint", "Perky", "Pine", "Pudding", "Ruby", "Scarlet", "Skittle", "Snappy", "Snow",
        "Snowball", "Snowdrop", "Snowflake", "Sparkle", "Sprinkle", "Sprinkles", "Starlight", "Stripes", "Sugar",
        "Sugarplum", "Tinkles", "Tinsel", "Tiny", "Topper", "Trinket", "Trixie", "Twinkle", "Twinkletoes", "Wink",
        "Winter", "Yule"
    };

    private static readonly string[] SurnameFirstParts =
    {
        "Angel", "Bustle", "Busy", "Candle", "Candy", "Carol", "Chill", "Chilly", "Chimney", "Chocolate", "Cider",
        "Cookie", "Crackle", "Cuddle", "Dream", "Ever", "Fire", "Flippy", "Frost", "Frosty", "Fruit", "Gift", "Good",
        "Goody", "Grotto", "Happy", "Holi", "Holly", "Hot", "Hustle", "Ivy", "Jiggle", "Jingle", "Jolly", "Magic",
        "Milk", "Milky", "Miracle", "Mistle", "Mitten", "Morning", "Muffin", "Nibble", "Night", "Nippy", "Party",
        "Pickle", "Plum", "Poem", "Pudding", "Rhyme", "Ribbon", "Sleepy", "Snow", "Sparkle", "Sugar", "Sweet", "Toffee",
        "Twinkle", "Wiggle"
    };

    private static readonly string[] SurnameSecondParts =
    {
        "ball", "beard", "bell", "bow", "box", "cake", "cane", "card", "carol",
        "cheer", "dance", "dancer", "dash", "feast", "flake", "foot", "friend", "frost", "fun", "game", "gift",
        "glitter", "glove", "guest", "hat", "hope", "hug", "icicle", "ivy", "joke", "joy", "jump", "kiss", "laugh",
        "light", "love", "milk", "mitten", "moon", "myrrh", "night", "pie", "plum", "scarf", "sledge", "sleigh", "song",
        "spirit", "star", "toy", "tree", "warmth", "wine", "wish", "wrap"
    };

    private static readonly Random Random = new Random(1);

    public static string GetName()
    {
        return $"{GetRandom(FirstNames)} {GetRandom(SurnameFirstParts)}{GetRandom(SurnameSecondParts)}";
    }

    private static T GetRandom<T>(IReadOnlyList<T> options)
    {
        return options[Random.Next(options.Count)];
    }
}